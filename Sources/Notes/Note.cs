namespace BandBrosClone;

using BandBrosClone.MusicNotation;
using Godot;
using System;

public partial class Note : NoteBase
{
	public PerformanceActionKind actionKind
	{
		get => _actionKind;
		private set => SetActionKind(value);
	}
	private PerformanceActionKind _actionKind;

	public ChartNoteHold chartNote { get; private set; }

	public override MidiTime Duration { get => _midiTime; protected set => SetBeat(value); }

	private MidiTime _midiTime;

	public Sprite2D attackSprite { get; private set; }
	public Sprite2D holdSprite { get; private set; }

	public Vector2 ReleasePosition => new Vector2(GlobalPosition.X + (float)Duration.ToSeconds(), GlobalPosition.Y);
	public Vector2 AttackPosition => GlobalPosition;

	public bool IsHolding { get; set; } = false;
	public bool HasReleased { get => _hasReleased; set { _hasReleased = value; Visible = !value; } }
	private bool _hasReleased = false;

	public const float HOLD_WIDTH = 150;

	public float Velocity { get; set; }

	public Note(PerformanceActionKind actionKind, MidiTime midiTime, float velocity, ChartNoteHold note)
	{
		_actionKind = actionKind;
		_midiTime = midiTime;
		Velocity = velocity;
		chartNote = note;
	}

	public void SetBeat(MidiTime midiTime)
	{
		_midiTime = Duration;
		if (holdSprite is null) return;
		if (midiTime.ToSeconds() < 0.5) return;
		holdSprite.Scale = new Vector2((float)(Duration.ToSeconds() * 0.5), holdSprite.Scale.Y);
	}

	public void MoveNote(double deltaTime)
	{
		MoveLocalX((float)-deltaTime * Velocity);
	}

	public override void _Ready()
	{
		attackSprite = new Sprite2D();
		attackSprite.ZIndex = 1;
		AddChild(attackSprite);

		holdSprite = new Sprite2D();
		holdSprite.Offset = new Vector2(HOLD_WIDTH, 0);
		holdSprite.Scale = new Vector2(0.5f, 0.7f);
		holdSprite.Texture = GD.Load<Texture2D>(Constants.NOTE_IMAGES_PATH.PathJoin("base.png"));
		holdSprite.ZIndex = 0;
		AddChild(holdSprite);

		attackSprite.MoveLocalX(-150);
		holdSprite.MoveLocalX(-150);

		SetActionKind(_actionKind);
		SetBeat(_midiTime);
	}

	public void SetActionKind(PerformanceActionKind actionKind)
	{
		_actionKind = actionKind;
		if (attackSprite is null) return;
		var imagePath = Constants.NOTE_IMAGES_PATH;
		switch (actionKind)
		{
			case PerformanceActionKind.I:
				{
					attackSprite.Texture = GD.Load<Texture2D>(imagePath.PathJoin("rect_down.png"));
					break;
				}
			case PerformanceActionKind.II:
				{
					attackSprite.Texture = GD.Load<Texture2D>(imagePath.PathJoin("rect_left.png"));
					break;
				}
			case PerformanceActionKind.III:
				{
					attackSprite.Texture = GD.Load<Texture2D>(imagePath.PathJoin("rect_up.png"));
					break;
				}
			case PerformanceActionKind.IV:
				{
					attackSprite.Texture = GD.Load<Texture2D>(imagePath.PathJoin("rect_right.png"));
					break;
				}
			case PerformanceActionKind.V:
				{
					attackSprite.Texture = GD.Load<Texture2D>(imagePath.PathJoin("circle_left.png"));
					break;
				}
			case PerformanceActionKind.VI:
				{
					attackSprite.Texture = GD.Load<Texture2D>(imagePath.PathJoin("circle_down.png"));
					break;
				}
			case PerformanceActionKind.VII:
				{
					attackSprite.Texture = GD.Load<Texture2D>(imagePath.PathJoin("circle_up.png"));
					break;
				}
			// TODO: IMPLEMENT SHARP AND OCTAVE UP
			case PerformanceActionKind.SHARP:
				{
					Visible = false;
					break;
				}
			case PerformanceActionKind.OCTAVE_UP:
				{
					Visible = false;
					break;
				}
		}
	}
}
