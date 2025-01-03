namespace BandBrosClone;

using Godot;
using Melanchall.DryWetMidi.Core;
using System.IO;


public partial class ChartCreatorDebugUi : Control
{
	private Chart chart;

	private LineEdit _midiFilePath;
	private Button _convertButton;
	private Button _pickMidiButton;

	private FileDialog _fileDialog;

	public override void _Ready()
	{

		_pickMidiButton = GetNode<Button>("%PickMidiButton");
		_pickMidiButton.Pressed += _pickMidiFileButtonPressed;

		_midiFilePath = GetNode<LineEdit>("%MidiFilePath");

		_convertButton = GetNode<Button>("%ConvertButton");
		_convertButton.Pressed += _onConvertButtonPressed;
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
		_fileDialog.Access = FileDialog.AccessEnum.Filesystem;

		_fileDialog.Filters = new string[] { "*.mid", "*.midi" };

		_fileDialog.Confirmed += () =>
		{
			_midiFilePath.Text = _fileDialog.CurrentPath;
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

		chart = Chart.CreateChartFromMidiFile(midiFile);

		GameManager.Info("Chart created from MIDI file.");

		foreach (var track in chart.Tracks)
		{
			foreach (var note in track.Notes)
			{
				GD.Print(note);
			}
		}
	}
}
