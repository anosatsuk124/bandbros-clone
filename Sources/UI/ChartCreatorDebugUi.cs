namespace BandBrosClone;

using BandBrosClone.MusicNotation;
using Godot;
using Melanchall.DryWetMidi.Core;
using System.IO;
using System.Linq;
using System.Text.Json;

public partial class ChartCreatorDebugUi : Control
{
	[Export] public PerformanceManager performanceManager;
	public PerformanceActionHandlerBase[] actionHandlers = new PerformanceActionHandlerBase[Constants.MAX_MIDI_CHANNEL_COUNT];

	private Chart _chart;

	private LineEdit _midiFilePath;
	private Button _convertButton;
	private Button _pickMidiButton;

	private Button _playButton;

	private FileDialog _fileDialog;

	public override void _Ready()
	{

		_pickMidiButton = GetNode<Button>("%PickMidiButton");
		_pickMidiButton.Pressed += _pickMidiFileButtonPressed;

		_midiFilePath = GetNode<LineEdit>("%MidiFilePath");

		_convertButton = GetNode<Button>("%ConvertButton");
		_convertButton.Pressed += _onConvertButtonPressed;

		_playButton = GetNode<Button>("%PlayButton");
		_playButton.Pressed += _onPlayButtonPressed;

		for (int i = 0; i < Constants.MAX_MIDI_CHANNEL_COUNT; i++)
		{
			actionHandlers[i] = new PerformanceActionHandlerBase();
			actionHandlers[i].performanceManager = performanceManager;
			AddChild(actionHandlers[i]);
		}
	}

	public override void _ExitTree()
	{
		if (_fileDialog is not null)
		{
			_fileDialog.QueueFree();
		}
	}

	private void _pickMidiFileButtonPressed()
	{
		_fileDialog = new FileDialog();
		AddChild(_fileDialog);

		_fileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
		_fileDialog.UseNativeDialog = true;
		_fileDialog.Access = FileDialog.AccessEnum.Filesystem;

		_fileDialog.Filters = new string[] { "*.mid", "*.midi" };

		_fileDialog.FileSelected += (selected) =>
		{
			_midiFilePath.Text = selected;
			_fileDialog.QueueFree();
		};

		_fileDialog.PopupCentered();
	}

	private void _onConvertButtonPressed()
	{
		var file = Godot.FileAccess.Open(_midiFilePath.Text, Godot.FileAccess.ModeFlags.Read);
		var bytes = file.GetBuffer((long)file.GetLength());
		var stream = new MemoryStream(bytes);
		var midiFile = MidiFile.Read(stream);

		_chart = Chart.CreateChartFromMidiFile(midiFile);

		GameManager.Info("Chart created from MIDI file.");
	}

	private async void _onPlayButtonPressed()
	{
		if (_chart is null)
		{
			GameManager.Warn("No chart to play.");
			return;
		}

		var sequencer = new ChartSequencer(_chart);

		foreach (var actionHandler in actionHandlers)
		{
			foreach (var chartTrack in _chart.Tracks)
			{
				GameManager.Info($"Setting scale to {chartTrack.Scale}");
				actionHandler.Scale = chartTrack.Scale;
			}
		}

		foreach (var (index, note) in sequencer)
		{
			if (!note.duration.Equals(0))
			{
				var duration = note.duration.time;
				await ToSignal(GetTree().CreateTimer(duration), Timer.SignalName.Timeout);
			}

			switch (note)
			{
				case ChartNoteOn on:
					{
						var actionKinds = PerformanceActionKindExtension.FromMidiNote(on.note.Note);
						foreach (var actionKind in actionKinds)
						{
							GameManager.Info(actionKind.ToActionName());
							actionHandlers[index].PerformHandler(new PerformanceAction(actionKind, true, false), on.note.Velocity);
						}
						break;
					}
				case ChartNoteOff off:
					{
						var actionKinds = PerformanceActionKindExtension.FromMidiNote(off.note.Note);
						foreach (var actionKind in actionKinds)
						{
							GameManager.Info(actionKind.ToActionName());
							actionHandlers[index].PerformHandler(new PerformanceAction(actionKind, false, true), new MidiNoteVelocity());
						}
						break;
					}
				default:
					{
						GameManager.Warn($"Unknown note type: {note.GetType().Name}");
						break;
					}
			}
		}

		await ToSignal(GetTree().CreateTimer(1), Timer.SignalName.Timeout);
		performanceManager.Reset();
	}
}
