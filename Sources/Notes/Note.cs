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

	public override MidiBeat Beat { get => _midiBeat; protected set => SetBeat(value); }

	private MidiBeat _midiBeat;

	public Sprite2D attackSprite { get; private set; }
	public Sprite2D holdSprite { get; private set; }

	public Note(PerformanceActionKind actionKind, MidiBeat midiBeat)
	{
		_actionKind = actionKind;
		_midiBeat = midiBeat;
	}

	public void SetBeat(MidiBeat midiBeat)
	{
		_midiBeat = midiBeat;
		if (holdSprite is null) return;
		holdSprite.Scale = new Vector2(midiBeat * 0.5f, 1);
	}

	public override void _Ready()
	{
		attackSprite = new Sprite2D();
		attackSprite.ZIndex = 1;
		holdSprite = new Sprite2D();
		holdSprite.Offset = new Vector2(150, 0);
		holdSprite.Scale = new Vector2(0.5f, 1);
		holdSprite.Texture = GD.Load<Texture2D>(Constants.NOTE_IMAGES_PATH.PathJoin("base.png"));
		holdSprite.ZIndex = 0;
		AddChild(attackSprite);
		AddChild(holdSprite);
		Scale = new Vector2(0.3f, 0.3f);
		SetActionKind(_actionKind);
		SetBeat(_midiBeat);
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
			default: throw new ArgumentOutOfRangeException();
		}
	}

	public override void _Process(double delta)
	{
	}
}
