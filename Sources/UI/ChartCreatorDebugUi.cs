namespace BandBrosClone;

using BandBrosClone.MusicNotation;
using Godot;
using Melanchall.DryWetMidi.Core;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


public partial class ChartCreatorDebugUi : Control
{
	[Export] public PerformanceManager performanceManager;
	public ActionHandlerBase[] actionHandlers = new ActionHandlerBase[Constants.MAX_MIDI_CHANNEL_COUNT];

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
			actionHandlers[i] = new PerformanceActionHandler();
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

		var json = JsonSerializer.Serialize(_chart);
		var outFile = Godot.FileAccess.Open(_midiFilePath.Text.GetBaseDir().PathJoin("chart.json"), Godot.FileAccess.ModeFlags.Write);
		outFile.StoreBuffer(json.ToUtf8Buffer());
	}

	private void _onPlayButtonPressed()
	{
		if (_chart is null)
		{
			GameManager.Warn("No chart to play.");
			return;
		}

		for (int idx = 0; idx < _chart.Tracks.Count; idx++)
		{
			sequencers.Add(new ChartTrackAutoPerformance(actionHandlers[idx], _chart.Tracks[idx]));
			AddChild(sequencers[idx]);
		}

		performanceManager.Reset();

		notesSequencer = new NotesSequencer(actionHandlers[3], _chart.Tracks[3]);
		notesSequencer.PostionOffset = new Vector2(0, 0);
		notesSequencer.Parent = notesSequencerParent;
		notesSequencer.DetectPointNode = detectPointNode;
		notesSequencerParent.AddChild(notesSequencer);

		for (int idx = 0; idx < sequencers.Count; idx++)
		{
			_playEnumerators.Add(sequencers[idx].Play(_chart.Tracks[idx].Notes));
		}

		isPlaying = true;
	}

	public override void _Process(double delta)
	{
		if (isPlaying)
		{
			foreach (var _ in _playEnumerators.Select(enumerator => enumerator.MoveNext())) ;
		}
	}

	private NotesSequencer notesSequencer;

	private List<ChartTrackSequencerBase> sequencers = new List<ChartTrackSequencerBase>();

	private List<IEnumerator> _playEnumerators = new List<IEnumerator>();
	private bool isPlaying = false;

	[Export] public Node2D notesSequencerParent;
	[Export] public Node2D detectPointNode;
}
