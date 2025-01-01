namespace BandBrosClone;

using System.Threading.Tasks;
using BandBrosClone.MusicNotation;
using Godot;
using MeltySynth;

[GlobalClass]
public partial class SoundfontPlayer : Node
{
	[Export] public AudioStreamPlayer Player { get; private set; }

	public Synthesizer Synthesizer { get; private set; } =
		new Synthesizer(Constants.DEFAULT_SOUNDFONT, _sampleHz);

	public MidiInstrumet[] Instruments { get; private set; } = new MidiInstrumet[Constants.MAX_MIDI_CHANNEL_COUNT];

	private AudioStreamGeneratorPlayback _playback; // Will hold the AudioStreamGeneratorPlayback.
	private static int _sampleHz = Constants.SAMPLE_RATE; // The sample rate of the sound wave.

	public SoundfontPlayer(AudioStreamPlayer player)
	{
		this.Player = player;
	}

	public override void _Ready()
	{
		if (Player.Stream is AudioStreamGenerator generator) // Type as a generator to access MixRate.
		{
			generator.BufferLength = 1f / Constants.RENDER_TO_BUFFER_RATE;
			generator.MixRate = _sampleHz;

			Player.Play();
			_playback = (AudioStreamGeneratorPlayback)Player.GetStreamPlayback();

			Task.Run(() =>
			{
				while (true)
				{
					_fillBuffer();
				}
			});
		}
	}

	public void PlayNoteOn(int channel, int note, int velocity)
	{
		Synthesizer.NoteOn(channel, note, velocity);
	}

	public void PlayNoteOff(int channel, int note)
	{
		Synthesizer.NoteOff(channel, note);
	}

	public void SetInstrument(MidiChannel channel, int bank, int program = 0)
	{
		Instruments[channel] = new MidiInstrumet(bank, program);
		Synthesizer.ProcessMidiMessage(channel, 0xC0, Instruments[channel].bank, Instruments[channel].program);
	}

	public void SetInstruments(MidiInstrumet[] midiInstrumets)
	{
		for (int i = 0; i < Constants.MAX_MIDI_CHANNEL_COUNT; i++)
		{
			SetInstrument(new MidiChannel(i), midiInstrumets[i].bank, midiInstrumets[i].program);
		}
	}

	private void _fillBuffer()
	{
		if (_playback == null)
		{
			return;
		}

		int framesAvailable = _playback.GetFramesAvailable();

		// The output buffer (3 seconds).
		var left = new float[framesAvailable];
		var right = new float[framesAvailable];

		Synthesizer.Render(left, right);

		for (int i = 0; i < framesAvailable; i++)
		{
			_playback.PushFrame(new Vector2(left[i], right[i]));
		}
	}
}


[GlobalClass]
public partial class SoundfontAudioStreamPlayer : AudioStreamPlayer
{
	public SoundfontAudioStreamPlayer() : base()
	{
		this.Stream = new AudioStreamGenerator();
	}
}