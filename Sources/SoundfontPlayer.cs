namespace BandBrosClone;

using System.Threading.Tasks;
using Godot;
using MeltySynth;

[GlobalClass]
public partial class SoundfontPlayer : Node
{
	[Export] public AudioStreamPlayer Player { get; set; }

	public Synthesizer Synthesizer =
		new Synthesizer(ResourceManager.GetSoundfontAbsPath(ProjectSettings.GetSetting("audio/soundfont_player/default_soundfont").AsString()), _sampleHz);


	private AudioStreamGeneratorPlayback _playback; // Will hold the AudioStreamGeneratorPlayback.
	private static int _sampleHz = ProjectSettings.GetSetting("audio/soundfont_player/sample_rate").AsInt32(); // The sample rate of the sound wave.

	public override void _Ready()
	{
		if (Player.Stream is AudioStreamGenerator generator) // Type as a generator to access MixRate.
		{
			generator.BufferLength = 1f / 120f;
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
