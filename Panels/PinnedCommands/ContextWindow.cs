using PopUpUI.Elements;
using PopUpUI.Surface;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using PopUpUI.Input;
using PopUpUI.Toolkit;
using SideKicker.Theming;

namespace SideKicker.Panels.PinnedCommands
{
    internal class ContextWindow : PopWindow
    {
        private const string SaveFileName = "pinnedcommands.json";
        private const int PanelTextSize = 12;

        private readonly IReadOnlyList<SavedCommand> _savedCommands;

        private bool _inEditingMode;
        private readonly Dictionary<CommandEditorTextBox, LinearTextBox> _textBoxes;
        private readonly TextPlaque[] _commandEditorTexts;
        private readonly TextedButton[] _addingCommandButtons;
        private readonly TextedButton[] _editingCommandButtons;

        private bool _inReorderMode;
        private int _reorderingButtonIndex;
        private readonly ScrollableButtonList _commandsList;

        public event Action<EditorButton> EditorButtonPressed;
        public event Action<int> CommandListButtonBodyPressed;
        public event Action<int> CommandListButtonEditPressed;
        public event Action<int, int> CommandListButtonsSwaped;

        public ContextWindow(IReadOnlyList<SavedCommand> savedCommands)
        {
            _savedCommands = savedCommands;

            // occopies an area of (0, 0, 220, 280)
            _textBoxes = new Dictionary<CommandEditorTextBox, LinearTextBox>();
            InitializeCommandEditor(out _commandEditorTexts, out _addingCommandButtons, out _editingCommandButtons);

            _commandsList = new ScrollableButtonList
            {
                Boundaries = new Rectangle(250, 30, 200, 230),
                ButtonHeight = CommandListLayoutCalculator.ButtonHeight,
                ButtonMargin = 4
            };
            _commandsList.ButtonPressed += CommandsList_ButtonPressed;

            this.Boundaries = new Rectangle(0, 0, 470, 260);
        }

        private void CommandsList_ButtonPressed(int buttonIndex)
        {
            CommandButtonLayout layout = CommandListLayoutCalculator.GetLayout(_commandsList.GetButtonBounds(buttonIndex), 
                                                                               _commandsList.RelativeMouseToHovered);

            if (layout == CommandButtonLayout.ReorderButton)
            {
                _inReorderMode = true;
                _reorderingButtonIndex = buttonIndex;
            }
            else if (layout == CommandButtonLayout.EditButton)
            {
                CommandListButtonEditPressed?.Invoke(buttonIndex);
            }
            else if (layout == CommandButtonLayout.Body)
            {
                CommandListButtonBodyPressed?.Invoke(buttonIndex);
            }
        }

        public void ShowCommand(SavedCommand? command)
        {
            if (command != null)
            {
                _textBoxes[CommandEditorTextBox.Name].Text = command.Name;
                _textBoxes[CommandEditorTextBox.PathToRunCommand].Text = command.PathToRunCommand;
                _textBoxes[CommandEditorTextBox.Command].Text = command.Command;
                _inEditingMode = true;
            }
            else
            {
                _textBoxes[CommandEditorTextBox.Name].Text = "";
                _textBoxes[CommandEditorTextBox.PathToRunCommand].Text = "";
                _textBoxes[CommandEditorTextBox.Command].Text = "";
                _inEditingMode = false;
            }
        }

        public SavedCommand CreateCommand()
        {
            return new SavedCommand
            {
                Name = _textBoxes[CommandEditorTextBox.Name].Text,
                PathToRunCommand = _textBoxes[CommandEditorTextBox.PathToRunCommand].Text,
                Command = _textBoxes[CommandEditorTextBox.Command].Text
            };
        }

        private void InitializeCommandEditor(out TextPlaque[] commandEditorTexts, out TextedButton[] addingCommandButtons, 
                                             out TextedButton[] editingCommandButtons)
        {
            _textBoxes.Add(CommandEditorTextBox.Name, new LinearTextBox
            {
                TextSize = PanelTextSize,
                Boundaries = new Rectangle(20, 70, 200, 40),
                SelectableMargin = 4
            });

            _textBoxes.Add(CommandEditorTextBox.PathToRunCommand, new LinearTextBox
            {
                TextSize = PanelTextSize,
                Boundaries = new Rectangle(20, 160, 200, 40),
                SelectableMargin = 4
            });

            _textBoxes.Add(CommandEditorTextBox.Command, new LinearTextBox
            {
                TextSize = PanelTextSize,
                Boundaries = new Rectangle(20, 250, 200, 40),
                SelectableMargin = 4
            });

            commandEditorTexts = new TextPlaque[]
            {
                new TextPlaque
                {
                    Text = "Visible Name Of Command:",
                    TextSize = PanelTextSize,
                    Position = new Vector2(20, 30)
                },
                new TextPlaque
                {
                    Text = "Directory To Run The Command In:",
                    TextSize = PanelTextSize,
                    Position = new Vector2(20, 120)
                },
                new TextPlaque
                {
                    Text = "Command To Run:",
                    TextSize = PanelTextSize,
                    Position = new Vector2(20, 210)
                }
            };

            addingCommandButtons = new TextedButton[]
            {
                new TextedButton
                {
                    Text = "Add",
                    Boundaries = new Rectangle(220 - 60, 220, 60, 40),
                    ClickCommand = new ActionCommander<Clickable>(_ => EditorButtonPressed?.Invoke(EditorButton.Add))
                }
            };

            editingCommandButtons = new TextedButton[]
            {
                new TextedButton
                {
                    Text = "Delete",
                    Boundaries = new Rectangle(140 - 60, 220, 60, 40),
                    ClickCommand = new ActionCommander<Clickable>(_ => EditorButtonPressed?.Invoke(EditorButton.Delete))
                },
                new TextedButton
                {
                    Text = "Save",
                    Boundaries = new Rectangle(220 - 60, 220, 60, 40),
                    ClickCommand = new ActionCommander<Clickable>(_ => EditorButtonPressed?.Invoke(EditorButton.Save))
                }
            };
        }

        protected override void RenderUpdate()
        {
            ResolvedTheme theme = ThemeService.CurrentTheme;
            Color textColor = theme.GetColor(ColorRegistry.TextColorId);

            DrawHelper.SetBeginBatch(WindowTexture, DrawHelper.AlphaBlendDrawOptions);

            DrawHelper.DrawBorderedBackground(new Rectangle(Point.Zero, this.Boundaries.Size), theme.GetColor(ColorRegistry.BorderId),
                                              theme.GetColor(ColorRegistry.SurfaceId), 1);

            foreach (var textPlaque in _commandEditorTexts)
            {
                FontService.Regular.DrawText(PanelTextSize, textPlaque.Text, textPlaque.Position, textColor);
            }

            foreach (var button in (_inEditingMode ? _editingCommandButtons : _addingCommandButtons))
            {
                DrawHelper.DrawBackground(button.Boundaries, theme.GetColor(ColorRegistry.BackgroundId));
                FontService.Regular.DrawCenteredText(PanelTextSize, button.Text, button.Boundaries, textColor);

                if (button.IsHovered)
                {
                    DrawHelper.DrawBackground(button.Boundaries, button.IsHeld ? theme.GetColor(ColorRegistry.HeldOverlayId) 
                                                                               : theme.GetColor(ColorRegistry.HoverOverlayId));
                }
            }

            DrawHelper.EndBatch();

            foreach (var textBox in _textBoxes.Values)
            {
                DrawHelper.Device.ScissorRectangle = textBox.Boundaries;
                DrawHelper.SetBeginBatch(WindowTexture, DrawHelper.AlphaScissoredDrawOptions);

                DrawHelper.DrawBorderedBackground(textBox.SelectableArea, theme.GetColor(ColorRegistry.BorderAccentId),
                                                  theme.GetColor(ColorRegistry.BackgroundId), 1);

                FontService.Regular.DrawVerticallyCenteredText(PanelTextSize, textBox.Text, 
                    new Vector2(textBox.Boundaries.X + textBox.TextOffset, textBox.Boundaries.Y), this.Boundaries.Height,
                    textColor);

                if (textBox.ReceivingInput)
                {
                    const int CaretWidth = 2;
                    int caretX = (int)FontService.Regular.MeasureWidth(PanelTextSize, textBox.Text[..textBox.CaretPosition]) + textBox.TextOffset;

                    DrawHelper.DrawBackground(new Rectangle(caretX, textBox.Boundaries.Y, CaretWidth, FontService.Regular.GetLineSpacing(PanelTextSize)), 
                                              textColor);
                }
                
                DrawHelper.EndBatch();
            }

            DrawHelper.Device.ScissorRectangle = _commandsList.Boundaries;
            DrawHelper.SetBeginBatch(WindowTexture, DrawHelper.AlphaScissoredDrawOptions);

            for (int i = 0; i < _savedCommands.Count; i++)
            {
                Rectangle relativeButtonBounds = _commandsList.GetButtonBounds(i);
                Rectangle buttonScreenBounds = new Rectangle(relativeButtonBounds.Location + _commandsList.Boundaries.Location, relativeButtonBounds.Size);

                // draw reorder icon here -> CommandLineLayoutCalculator.ReorderButtonBounds(buttonScreenBounds)
                FontService.Regular.DrawVerticallyCenteredText(PanelTextSize, _savedCommands[i].Name, 
                                                               CommandListLayoutCalculator.ButtonNamePosition(buttonScreenBounds), 
                                                               relativeButtonBounds.Height, textColor);
                // draw edit icon here -> CommandLineLayoutCalculator.EditButtonBounds(buttonScreenBounds)

                if (_commandsList.HoveredButtonIndex == i)
                {
                    DrawHelper.DrawBackground(buttonScreenBounds, _commandsList.IsHoveredButtonHeld ? theme.GetColor(ColorRegistry.HeldOverlayId)
                                                                                                    : theme.GetColor(ColorRegistry.HoverOverlayId));
                }
            }

            DrawHelper.EndBatch();

            base.RenderUpdate();
        }

        protected override void OnMouseButtonStateChange(PopWindowMouseEventArgs e)
        {
            _commandsList.MouseStateChangeUpdate(e);
            base.OnMouseButtonStateChange(e);
        }

        protected override void OnMousePositionChange(PopWindowMouseEventArgs e)
        {
            if (_inReorderMode)
            {
                int onButton = _commandsList.GetButtonOn(e.MousePosition - _commandsList.Boundaries.Location);
                if (onButton != -1 && onButton != _reorderingButtonIndex)
                {
                    CommandListButtonsSwaped?.Invoke(onButton, _reorderingButtonIndex);
                    _reorderingButtonIndex = onButton;
                }
            }
            else
            {
                _commandsList.MousePositionChanged(e);
            }

            base.OnMousePositionChange(e);
        }

        protected override void OnMouseVerticalScrollChange(PopWindowMouseEventArgs e, int change)
        {
            if (_commandsList.Boundaries.Contains(e.MousePosition))
            {
                _commandsList.Scroll(e, -change / 6);
            }

            base.OnMouseVerticalScrollChange(e, change);
        }
    }
}
