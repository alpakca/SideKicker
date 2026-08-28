using PopUpUI.Elements;
using PopUpUI.Surface;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using PopUpUI.Input;
using PopUpUI.Toolkit;
using SideKicker.Theming;
using System.Collections.ObjectModel;

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

        public ContextWindow(ObservableCollection<SavedCommand> savedCommands)
        {
            _savedCommands = savedCommands;

            // occopies an area of (0, 0, 220, 250)
            _textBoxes = new Dictionary<CommandEditorTextBox, LinearTextBox>();
            InitializeCommandEditor(out _commandEditorTexts, out _addingCommandButtons, out _editingCommandButtons);

            _commandsList = new ScrollableButtonList
            {
                Boundaries = new Rectangle(270, 20, 200, 210),
                ButtonHeight = CommandListLayoutCalculator.ButtonHeight,
                ButtonMargin = 4
            };
            _commandsList.ButtonPressed += CommandsList_ButtonPressed;
            _commandsList.ButtonHeld += CommandsList_ButtonHeld;
            savedCommands.CollectionChanged += (sender, _) => _commandsList.ButtonCount = ((IReadOnlyList<SavedCommand>)sender).Count;

            this.Boundaries = new Rectangle(0, 0, 490, 250);
            this.Elements.AddRange(_textBoxes.Values);
        }

        private void CommandsList_ButtonPressed(int buttonIndex)
        {
            CommandButtonLayout layout = CommandListLayoutCalculator.GetLayout(_commandsList.GetButtonBounds(buttonIndex).Size, 
                                                                               _commandsList.RelativeMouseToHovered);

            if (layout == CommandButtonLayout.EditButton)
            {
                CommandListButtonEditPressed?.Invoke(buttonIndex);
            }
            else if (layout == CommandButtonLayout.Body)
            {
                CommandListButtonBodyPressed?.Invoke(buttonIndex);
            }
        }

        private void CommandsList_ButtonHeld(int buttonIndex)
        {
            CommandButtonLayout layout = CommandListLayoutCalculator.GetLayout(_commandsList.GetButtonBounds(buttonIndex).Size,
                                                                   _commandsList.RelativeMouseToHovered);

            if (layout == CommandButtonLayout.ReorderButton)
            {
                _inReorderMode = true;
                _reorderingButtonIndex = buttonIndex;
            }
        }

        public void ShowCommand(SavedCommand? command)
        {
            if (command != null)
            {
                _textBoxes[CommandEditorTextBox.Name].Text = command.Name;
                _textBoxes[CommandEditorTextBox.PathToRunCommand].Text = command.PathToRunCommand;
                _textBoxes[CommandEditorTextBox.FileName].Text = command.FileName;

                if (!_inEditingMode)
                {
                    foreach (var button in _addingCommandButtons) this.Elements.Remove(button);
                }

                _inEditingMode = true;
                this.Elements.AddRange(_editingCommandButtons);
            }
            else
            {
                _textBoxes[CommandEditorTextBox.Name].Text = "";
                _textBoxes[CommandEditorTextBox.PathToRunCommand].Text = "";
                _textBoxes[CommandEditorTextBox.FileName].Text = "";

                if (_inEditingMode)
                {
                    foreach (var button in _editingCommandButtons) this.Elements.Remove(button);
                }

                _inEditingMode = false;
                this.Elements.AddRange(_addingCommandButtons);
            }
        }

        public SavedCommand CreateCommand()
        {
            return new SavedCommand
            {
                Name = _textBoxes[CommandEditorTextBox.Name].Text,
                PathToRunCommand = _textBoxes[CommandEditorTextBox.PathToRunCommand].Text,
                FileName = _textBoxes[CommandEditorTextBox.FileName].Text
            };
        }

        private void InitializeCommandEditor(out TextPlaque[] commandEditorTexts, out TextedButton[] addingCommandButtons, 
                                             out TextedButton[] editingCommandButtons)
        {
            _textBoxes.Add(CommandEditorTextBox.Name, new LinearTextBox
            {
                TextSize = PanelTextSize,
                Boundaries = new Rectangle(20, 40, 200, 30),
                SelectableMargin = 4
            });

            _textBoxes.Add(CommandEditorTextBox.PathToRunCommand, new LinearTextBox
            {
                TextSize = PanelTextSize,
                Boundaries = new Rectangle(20, 100, 200, 30),
                SelectableMargin = 4
            });

            _textBoxes.Add(CommandEditorTextBox.FileName, new LinearTextBox
            {
                TextSize = PanelTextSize,
                Boundaries = new Rectangle(20, 160, 200, 30),
                SelectableMargin = 4
            });

            commandEditorTexts = new TextPlaque[]
            {
                new TextPlaque
                {
                    Text = "Visible Name Of Command:",
                    TextSize = PanelTextSize,
                    Position = new Vector2(20, 20)
                },
                new TextPlaque
                {
                    Text = "Directory To Open File:",
                    TextSize = PanelTextSize,
                    Position = new Vector2(20, 80)
                },
                new TextPlaque
                {
                    Text = "File Path:",
                    TextSize = PanelTextSize,
                    Position = new Vector2(20, 140)
                }
            };

            addingCommandButtons = new TextedButton[]
            {
                new TextedButton
                {
                    Text = "Add",
                    Boundaries = new Rectangle(220 - 90, 200, 90, 30),
                    ClickCommand = new ActionCommander<Clickable>(_ => EditorButtonPressed?.Invoke(EditorButton.Add)),
                    SelectablePacket = new SelectablePacket(),
                    HoldPacket = new HoldablePacket()
                }
            };

            editingCommandButtons = new TextedButton[]
            {
                new TextedButton
                {
                    Text = "Delete",
                    Boundaries = new Rectangle(20, 200, 90, 30),
                    ClickCommand = new ActionCommander<Clickable>(_ => EditorButtonPressed?.Invoke(EditorButton.Delete)),
                    SelectablePacket = new SelectablePacket(),
                    HoldPacket = new HoldablePacket()
                },
                new TextedButton
                {
                    Text = "Save",
                    Boundaries = new Rectangle(220 - 90, 200, 90, 30),
                    ClickCommand = new ActionCommander<Clickable>(_ => EditorButtonPressed?.Invoke(EditorButton.Save)),
                    SelectablePacket = new SelectablePacket(),
                    HoldPacket = new HoldablePacket()
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

            DrawHelper.DrawBackground(new Rectangle(250, 20, 1, this.Boundaries.Height - 20 * 2), theme.GetColor(ColorRegistry.BorderId));

            DrawHelper.EndBatch();

            foreach (var textBox in _textBoxes.Values)
            {
                DrawHelper.Device.ScissorRectangle = textBox.Boundaries;
                DrawHelper.SetBeginBatch(WindowTexture, DrawHelper.AlphaScissoredDrawOptions);

                DrawHelper.DrawBorderedBackground(textBox.SelectableArea, theme.GetColor(ColorRegistry.BorderAccentId),
                                                  theme.GetColor(ColorRegistry.BackgroundId), 1);

                FontService.Regular.DrawVerticallyCenteredText(PanelTextSize, textBox.Text, 
                    new Vector2(textBox.Boundaries.X + textBox.TextOffset, textBox.Boundaries.Y), textBox.Boundaries.Height,
                    textColor);

                if (textBox.ReceivingInput)
                {
                    const int CaretWidth = 2;
                    int caretX = (int)FontService.Regular.MeasureWidth(PanelTextSize, textBox.Text[..textBox.CaretPosition]) + textBox.TextOffset;

                    int caretHeight = FontService.Regular.GetLineSpacing(PanelTextSize);
                    DrawHelper.DrawBackground(new Rectangle(textBox.Boundaries.X + caretX, textBox.Boundaries.Y + (textBox.Boundaries.Height - caretHeight) / 2,
                                                            CaretWidth, caretHeight), 
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

                if (i == _commandsList.HoveredButtonIndex)
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
            if (e.ReleasedButtons.HasLeft)
            {
                _inReorderMode = false;
            }

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

            _commandsList.MousePositionChanged(e);
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
