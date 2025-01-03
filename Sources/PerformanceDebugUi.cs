namespace BandBrosClone;

using Godot;
using MusicScale = MusicNotation.Scale;
using MusicNotation;

public partial class PerformanceDebugUi : Control
{
	[Export] public PerformanceManager PerformanceManager { get; set; }
	private Button _toggleButton;

	private Control _toToggleNode;

	private LineEdit _keyInput;
	private LineEdit _bankInput;
	private LineEdit _programInput;

	public override void _Ready()
	{
		_toggleButton = GetNode<Button>("%ToggleButton");
		_toToggleNode = _toggleButton.GetChild<Control>(0);
		_toggleButton.Pressed += () => _toToggleNode.Visible = !_toToggleNode.Visible;

		_keyInput = GetNode<LineEdit>("%KeyInput");
		_keyInput.TextChanged += _onKeyInputTextChanged;

		_bankInput = GetNode<LineEdit>("%BankInput");
		_bankInput.TextChanged += _onBankInputTextChanged;

		_programInput = GetNode<LineEdit>("%ProgramInput");
		_programInput.TextChanged += _onProgramInputTextChanged;
	}

	private void _onProgramInputTextChanged(string newText)
	{
		if (PerformanceManager is null) return;

		if (int.TryParse(newText, out int program))
		{
			var bank = PerformanceManager.Instruments[PerformanceManager.PlayerChannel].bank;

			PerformanceManager.SetInstrument(PerformanceManager.PlayerChannel, bank, program);
		}
	}

	private void _onBankInputTextChanged(string newText)
	{
		if (PerformanceManager is null) return;

		if (int.TryParse(newText, out int bank))
		{
			PerformanceManager.SetInstrument(PerformanceManager.PlayerChannel, bank);
		}
	}

	private void _onKeyInputTextChanged(string newText)
	{
		if (PerformanceManager is null) return;

		if (int.TryParse(newText, out int key))
		{
			PerformanceManager.Scale = MusicScale.Major(new TonalKey(new MidiNote(key)));
		}
	}
}
