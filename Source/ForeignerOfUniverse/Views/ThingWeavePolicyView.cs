using ForeignerOfUniverse.Models;
using ForeignerOfUniverse.Windows;
using Nebulae.RimWorld.UI.Controls;
using Nebulae.RimWorld.UI.Controls.Basic;
using Nebulae.RimWorld.UI.Controls.Composites;
using Nebulae.RimWorld.UI.Controls.Panels;
using Nebulae.RimWorld.UI.Core.Data.Bindings;
using Nebulae.RimWorld.UI.Core.Events;
using Nebulae.RimWorld.UI.Utilities;
using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;
using Grid = Nebulae.RimWorld.UI.Controls.Panels.Grid;
using Label = Nebulae.RimWorld.UI.Controls.Basic.Label;

namespace ForeignerOfUniverse.Views
{
    internal sealed class ThingWeavePolicyView : CompositeControl, IRenameable
    {
        public readonly ThingWeavePolicy Model;


        //------------------------------------------------------
        //
        //  Public Properties
        //
        //------------------------------------------------------

        #region Public Properties

        public string BaseLabel => "Untitled".Translate();

        public string InspectLabel => Model.Label;

        public string RenamableLabel
        {
            get => _nameLabel.Text;
            set => _nameLabel.Text = value;
        }

        public bool Selected
        {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    if (_selected)
                    {
                        _background.Background = CursorDirectlyOver ? BrushUtility.DarkGrey : BrushUtility.DarkerGrey;
                        _selected = false;
                    }
                    else
                    {
                        _background.Background = TexUI.HighlightSelectedTex;
                        _selected = true;
                    }
                }
            }
        }

        #endregion


        public ThingWeavePolicyView(ThingWeavePolicy model)
        {
            AllowDrag = true;
            IsHitTestVisible = true;
            Model = model;
            Name = model.Label;

            _background = new Border
            {
                Background = BrushUtility.DarkerGrey,
                BorderBrush = BrushUtility.LightGrey,
                BorderThickness = 1f
            };

            _nameLabel = new Label
            {
                Anchor = TextAnchor.MiddleLeft,
                Text = model.Label,
                Margin = new Thickness(4f, 0f, 0f, 0f)
            };

            Binding.Create(_nameLabel, Label.TextProperty, Model, nameof(ThingWeavePolicy.Label), BindingMode.OneWay, BindingFlags.Instance | BindingFlags.Public);
            Binding.Create(_nameLabel, Label.TextProperty, this, nameof(Name), BindingMode.OneWay, BindingFlags.Instance | BindingFlags.Public);
            Initialize();
        }


        //------------------------------------------------------
        //
        //  Public Static Methods
        //
        //------------------------------------------------------

        #region Public Static Methods

        public static ThingWeavePolicyView Convert(ThingWeavePolicy model)
        {
            return new ThingWeavePolicyView(model);
        }

        public static ThingWeavePolicy GetModel(ThingWeavePolicyView view)
        {
            return view.Model;
        }

        #endregion


        //------------------------------------------------------
        //
        //  Protected Methods
        //
        //------------------------------------------------------

        #region Protected Methods

        protected override Control CreateContent()
        {
            var renameButton = new IconButton(TexButton.Rename)
            {
                Tooltip = "RenamePolicyTip".Translate()
            };
            renameButton.Click += OnRename;

            var deleteButton = new IconButton(TexButton.Delete)
            {
                Tooltip = "FOU.ThingInfo.Weave.Delete.Tooltip".Translate(Name.Colorize(ColoredText.NameColor))
            };
            deleteButton.Click += OnDelete;

            _background.Content = new Grid()
                .DefineColumns(Grid.Remain, 34f, 34f)
                .Set(_nameLabel, renameButton, deleteButton);

            return _background;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            if (e.Target is ThingWeavePolicyView view && this.TryFindPartent<StackPanel>(out var panel))
            {
                panel.Insert(view, this);
                RestoreValue(OpacityProperty);
                e.Handled = true;
            }
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            SetValueTemporarily(OpacityProperty, 0.3f);
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            RestoreValue(OpacityProperty);
        }

        protected override void OnDragStart(DragEventArgs e)
        {
            SetValueTemporarily(OpacityProperty, 0.3f);
        }

        protected override void OnDragStop(DragEventArgs e)
        {
            RestoreValue(OpacityProperty);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (_selected || !CursorDirectlyOver)
            {
                return;
            }

            _background.Background = TexUI.HighlightSelectedTex;
            _selected = true;

            if (this.TryFindPartent<ThingWeaveView>(out var view))
            {
                SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                view.Select(this);
            }
        }

        protected override void OnMouseEnter(RoutedEventArgs e)
        {
            if (!_selected)
            {
                SoundDefOf.Mouseover_Standard.PlayOneShotOnCamera();

                _background.Background = BrushUtility.DarkGrey;
            }
        }

        protected override void OnMouseLeave(RoutedEventArgs e)
        {
            if (!_selected)
            {
                _background.Background = BrushUtility.DarkerGrey;
            }
        }


        #endregion


        //------------------------------------------------------
        //
        //  Private Methods
        //
        //------------------------------------------------------

        #region Private Methods

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            if (LayoutManager.Owner is ThingWeaveWindow window)
            {
                window.Comp.weavePolicies.Remove(Model);
            }

            if (this.TryFindPartent<ThingWeaveView>(out var view))
            {
                view.Remove(this);
            }
        }

        private void OnRename(object sender, RoutedEventArgs e)
        {
            new ThingWeavePolicyRenameWindow(this).Show();
        }

        #endregion


        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        #region Private Fields

        private readonly Border _background;
        private readonly Label _nameLabel;

        private bool _selected;

        #endregion
    }
}
