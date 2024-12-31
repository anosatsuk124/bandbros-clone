namespace BandBrosClone;

using System.IO;
using System.Threading.Tasks;
using Godot;
using MeltySynth;

[GlobalClass]
public partial class SoundfontPlayer : Node
{
	[Export] public AudioStreamPlayer Player { get; set; }

	private AudioStreamGeneratorPlayback _playback; // Will hold the AudioStreamGeneratorPlayback.
	private int _sampleHz;
	private float _pulseHz = 440.0f; // The frequency of the sound wave.

	private Synthesizer _synthesizer;

	private double _timeBegin;
	private double _timeDelay;

	private Timer _playTimer;


	public override void _Ready()
	{
		if (Player.Stream is AudioStreamGenerator generator) // Type as a generator to access MixRate.
		{
			generator.BufferLength = 1f / 60f; // 100ms buffer.
			_sampleHz = (int)generator.MixRate;

			Player.Play();
			_playback = (AudioStreamGeneratorPlayback)Player.GetStreamPlayback();

			GD.Print("GetSoundfontAbsPath: ", ResourceManager.GetSoundfontAbsPath("TimGM6mb.sf2"), File.Exists(ResourceManager.GetSoundfontAbsPath("TimGM6mb.sf2")));

			_synthesizer = new Synthesizer(ResourceManager.GetSoundfontAbsPath("TimGM6mb.sf2"), _sampleHz);

			Task.Run(() =>
			{
				while (true)
				{
					FillBuffer();
				}
			});
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_up"))
		{
			// Play some notes (middle C, E, G).
			_synthesizer.NoteOn(0, 60, 100);
			// _synthesizer.NoteOn(0, 64, 100);
			// _synthesizer.NoteOn(0, 67, 100);
		}

		if (@event.IsActionReleased("ui_up"))
		{
			_synthesizer.NoteOff(0, 60);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
	}


	public void FillBuffer()
	{
		int framesAvailable = _playback.GetFramesAvailable();

		// The output buffer (3 seconds).
		var left = new float[framesAvailable];
		var right = new float[framesAvailable];

		_synthesizer.Render(left, right);

		for (int i = 0; i < framesAvailable; i++)
		{
			_playback.PushFrame(new Vector2(left[i], right[i]));
		}
	}
}
