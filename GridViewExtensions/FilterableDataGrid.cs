using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NbuExplorer.GridViewExtensions.GridFilterFactories;
using NbuExplorer.GridViewExtensions.GridFilters;

namespace NbuExplorer.GridViewExtensions
{
    /// <summary>
    ///     Control which embeds an <see cref="DataGridView" /> and a
    ///     <see cref="DataGridFilterExtender" /> for providing automatic
    ///     filtering on all visible columns.
    /// </summary>
    public class FilterableDataGrid : UserControl
    {
        private DataGridFilterExtender _extender;
        private IContainer components;

        /// <summary>
        ///     Publishes the embedded <see cref="DataGridView" /> to allow
        ///     full control over its settings.
        /// </summary>
        [Browsable(false)] public DataGridView EmbeddedDataGridView;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public FilterableDataGrid()
        {
            InitializeComponent();

            RepositionGrid();
        }

        /// <summary>
        ///     filter not related to extended UI, will be ANDed to UI filters
        /// </summary>
        public string CustomFilter
        {
            get { return _extender.CustomFilter; }
            set { _extender.CustomFilter = value; }
        }

        /// <summary>
        ///     Gets and sets whether the filter criteria is automatically refreshed when
        ///     changes are made to the filter controls. If set to false then a call to
        ///     <see cref="RefreshFilters" /> is needed to manually refresh the criteria.
        /// </summary>
        [Browsable(true), DefaultValue(RefreshMode.OnInput)]
        [Description("Specifies if the view automatically refreshes to reflect "
                     + "changes in the grid filter controls.")]
        public RefreshMode AutoRefreshMode
        {
            get { return _extender.AutoRefreshMode; }
            set { _extender.AutoRefreshMode = value; }
        }

        /// <summary>
        ///     Gets and sets whether filters are kept while switching between different tables.
        /// </summary>
        [Browsable(true), DefaultValue(false)]
        [Description("Specifies whether filters are kept while switching between different tables.")]
        public bool KeepFilters
        {
            get { return _extender.KeepFilters; }
            set { _extender.KeepFilters = value; }
        }

        /// <summary>
        ///     Gets and sets the poisiton of the filter GUI elements.
        /// </summary>
        [Browsable(true), DefaultValue(FilterPosition.Top)]
        [Description("Gets and sets the position of the filter GUI elements.")]
        public FilterPosition FilterBoxPosition
        {
            get { return _extender.FilterBoxPosition; }
            set
            {
                _extender.FilterBoxPosition = value;
                RepositionGrid();
            }
        }

        /// <summary>
        ///     Gets and sets the text for the filter label.
        /// </summary>
        [Browsable(true), DefaultValue("Filter")]
        [Description("Gets and sets the text for the filter label.")]
        public string FilterText
        {
            get { return _extender.FilterText; }
            set { _extender.FilterText = value; }
        }

        /// <summary>
        ///     Gets and sets the <see cref="IGridFilterFactory" /> used to generate the filter GUI.
        /// </summary>
        [Browsable(true), DefaultValue(null)]
        [Description("Gets and sets factory instance which should be "
                     + "used to create grid filters.")]
        public IGridFilterFactory FilterFactory
        {
            get { return _extender.FilterFactory; }
            set { _extender.FilterFactory = value; }
        }

        /// <summary>
        ///     Gets and sets whether the filter label should be visible.
        /// </summary>
        [Browsable(true), DefaultValue(true)]
        [Description("Gets and sets whether the filter label should be visible.")]
        public bool FilterTextVisible
        {
            get { return _extender.FilterTextVisible; }
            set { _extender.FilterTextVisible = value; }
        }

        /// <summary>
        ///     The selected operator to combine the filter criterias.
        /// </summary>
        [Browsable(true), DefaultValue(LogicalOperators.And)]
        [Description("The selected operator to combine the filter criterias.")]
        public LogicalOperators Operator
        {
            get { return _extender.Operator; }
            set { _extender.Operator = value; }
        }

        /// <summary>
        ///     Gets and sets the <see cref="IBindingListView" /> which should be displayed in the grid.
        ///     This is needed because only <see cref="IBindingListView" />s provide in built mechanisms
        ///     to filter their content.
        /// </summary>
        [Browsable(true), DefaultValue(null)]
        [Description("The IBindingListView which should be initially displayed.")]
        public IBindingListView DataSource
        {
            get { return EmbeddedDataGridView.DataSource as IBindingListView; }
            set
            {
                _extender.BeginInit();
                EmbeddedDataGridView.DataSource = value;
                _extender.EndInit();
            }
        }

        /// <summary>
        ///     Gets and sets what information is shown to the user
        ///     if an error in the builded filter criterias occurs.
        /// </summary>
        [Browsable(true), DefaultValue(FilterErrorModes.General)]
        [Description("Specifies what information is shown to the user "
                     + "if an error in the builded filter criterias occurs.")]
        public FilterErrorModes MessageErrorMode
        {
            get { return _extender.MessageErrorMode; }
            set { _extender.MessageErrorMode = value; }
        }

        /// <summary>
        ///     Gets and sets what information is printed to the console
        ///     if an error in the builded filter criterias occurs.
        /// </summary>
        [Browsable(true), DefaultValue(FilterErrorModes.Off)]
        [Description("Specifies what information is printed to the console "
                     + "if an error in the builded filter criterias occurs.")]
        public FilterErrorModes ConsoleErrorMode
        {
            get { return _extender.ConsoleErrorMode; }
            set { _extender.ConsoleErrorMode = value; }
        }

        /// <summary>
        ///     Gets a modifyable collection which maps <see cref="DataTable.TableName" />s
        ///     to base filter strings which are applied in front of the automatically
        ///     created filter.
        /// </summary>
        /// <remarks>
        ///     The grid contents is not automatically refreshed when modifying this
        ///     collection. A call to <see cref="RefreshFilters" /> is needed for this.
        /// </remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public StringDictionary BaseFilters
        {
            get { return _extender.BaseFilters; }
        }

        /// <summary>
        ///     Gets or sets which operator should be used to combine the base filter
        ///     with the automatically created filters.
        /// </summary>
        [Browsable(true), DefaultValue(LogicalOperators.And)]
        [Description("Operator which should be used to combine the base filter "
                     + "with the automatically created filters.")]
        public LogicalOperators BaseFilterOperator
        {
            get { return _extender.BaseFilterOperator; }
            set { _extender.BaseFilterOperator = value; }
        }

        /// <summary>
        ///     Gets or sets whether base filters should be used when refreshing
        ///     the filter criteria. Setting it to false will disable the functionality
        ///     while still keeping the base filter strings in the <see cref="BaseFilters" />
        ///     collection intact.
        /// </summary>
        [Browsable(true), DefaultValue(true)]
        [Description("Gets or sets whether base filters should be used when "
                     + "refreshing the filter criteria.")]
        public bool BaseFilterEnabled
        {
            get { return _extender.BaseFilterEnabled; }
            set { _extender.BaseFilterEnabled = value; }
        }

        /// <summary>
        ///     Gets or sets the currently used base filter. Internally it adjusts the
        ///     <see cref="BaseFilters" /> collection with the given value and the current
        ///     <see cref="DataTable.TableName" /> and also initiates a refresh.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CurrentTableBaseFilter
        {
            get { return _extender.CurrentTableBaseFilter; }
            set { _extender.CurrentTableBaseFilter = value; }
        }

        /// <summary>
        ///     Erforderliche Methode f�r die Designerunterst�tzung.
        ///     Der Inhalt der Methode darf nicht mit dem Code-Editor ge�ndert werden.
        /// </summary>
        private void InitializeComponent()
        {
            components = new Container();
            var defaultGridFilterFactory1 = new DefaultGridFilterFactory();
            EmbeddedDataGridView = new DataGridView();
            _extender = new DataGridFilterExtender(components);
            ((ISupportInitialize) (EmbeddedDataGridView)).BeginInit();
            ((ISupportInitialize) (_extender)).BeginInit();
            SuspendLayout();
            //
            // _grid
            //
            EmbeddedDataGridView.Location = new Point(0, 24);
            EmbeddedDataGridView.Name = "EmbeddedDataGridView";
            EmbeddedDataGridView.Size = new Size(496, 352);
            EmbeddedDataGridView.TabIndex = 0;
            EmbeddedDataGridView.MouseDown += OnMouseDown;
            EmbeddedDataGridView.KeyDown += OnKeyDown;
            EmbeddedDataGridView.MouseMove += OnMouseMove;
            EmbeddedDataGridView.PreviewKeyDown += OnPreviewKeyDown;
            EmbeddedDataGridView.MouseEnter += OnMouseEnter;
            EmbeddedDataGridView.MouseHover += OnMouseHover;
            EmbeddedDataGridView.MouseLeave += OnMouseLeave;
            EmbeddedDataGridView.KeyUp += OnKeyUp;
            EmbeddedDataGridView.MouseUp += OnMouseUp;
            EmbeddedDataGridView.KeyPress += OnKeyPress;
            EmbeddedDataGridView.DoubleClick += OnDoubleClick;
            //
            // _extender
            //
            _extender.DataGridView = EmbeddedDataGridView;
            defaultGridFilterFactory1.CreateDistinctGridFilters = false;
            defaultGridFilterFactory1.DefaultGridFilterType = typeof (TextGridFilter);
            defaultGridFilterFactory1.DefaultShowDateInBetweenOperator = false;
            defaultGridFilterFactory1.DefaultShowNumericInBetweenOperator = false;
            defaultGridFilterFactory1.HandleEnumerationTypes = true;
            defaultGridFilterFactory1.MaximumDistinctValues = 20;
            _extender.FilterFactory = defaultGridFilterFactory1;
            _extender.GridFilterBound += OnGridFilterBound;
            _extender.GridFilterUnbound += OnGridFilterUnbound;
            _extender.AfterFiltersChanged += OnAfterFiltersChanged;
            _extender.BeforeFiltersChanging += OnBeforeFiltersChanging;
            //
            // FilterableDataGrid
            //
            Controls.Add(EmbeddedDataGridView);
            Name = "FilterableDataGrid";
            Size = new Size(496, 376);
            ((ISupportInitialize) (EmbeddedDataGridView)).EndInit();
            ((ISupportInitialize) (_extender)).EndInit();
            ResumeLayout(false);
        }

        /// <summary>
        ///     Event, which gets fired whenever the filter criteria has been changed.
        /// </summary>
        public event EventHandler AfterFiltersChanged;

        /// <summary>
        ///     Event, which gets fired whenever the filter criteria are going to be changed.
        /// </summary>
        public event EventHandler BeforeFiltersChanging;

        /// <summary>
        ///     Event, which gets fired whenever an <see cref="IGridFilter" /> has been bound
        ///     and thus added to this instance.
        /// </summary>
        public event GridFilterEventHandler GridFilterBound;

        /// <summary>
        ///     Event, which gets fired whenever an <see cref="IGridFilter" /> has been unbound
        ///     and thus removed to this instance.
        /// </summary>
        public event GridFilterEventHandler GridFilterUnbound;

        /// <summary>
        ///     Gets all currently set <see cref="IGridFilter" />s.
        /// </summary>
        /// <returns>Collection of <see cref="IGridFilter" />s.</returns>
        public GridFilterCollection GetGridFilters()
        {
            return _extender.GetGridFilters();
        }

        /// <summary>
        ///     Clears all filters to initial state.
        /// </summary>
        public void ClearFilters()
        {
            _extender.ClearFilters();
        }

        /// <summary>
        ///     Gets all filters currently set
        /// </summary>
        /// <returns></returns>
        public string[] GetFilters()
        {
            return _extender.GetFilters();
        }

        /// <summary>
        ///     Sets all filters to the specified values.
        ///     The values must be in order of the column styles in the current view.
        ///     This function should normally be used with data previously coming
        ///     from the <see cref="GetFilters" /> function.
        /// </summary>
        /// <param name="filters">filters to set</param>
        public void SetFilters(string[] filters)
        {
            _extender.SetFilters(filters);
        }

        /// <summary>
        ///     Refreshes the filter criteria to match the current contents of the associated
        ///     filter controls.
        /// </summary>
        public void RefreshFilters()
        {
            _extender.RefreshFilters();
        }

        private void RepositionGrid()
        {
            var newTop = EmbeddedDataGridView.Top;
            var newHeight = EmbeddedDataGridView.Height;
            var newLeft = 0;
            var newWidth = Width;
            switch (_extender.FilterBoxPosition)
            {
                case FilterPosition.Off:
                    newTop = 0;
                    newHeight = Height;
                    break;

                case FilterPosition.Top:
                    newTop = _extender.NeededControlHeight + 1;
                    newHeight = Height - newTop - 1;
                    break;

                case FilterPosition.Bottom:
                    newTop = 0;
                    newHeight = Height - _extender.NeededControlHeight - 1;
                    break;
            }

            EmbeddedDataGridView.SetBounds(newLeft, newTop, newWidth, newHeight, BoundsSpecified.All);
        }

        private void OnAfterFiltersChanged(object sender, EventArgs e)
        {
            OnAfterFiltersChanged(e);
        }

        private void OnBeforeFiltersChanging(object sender, EventArgs e)
        {
            OnBeforeFiltersChanging(e);
        }

        private void OnGridFilterBound(object sender, GridFilterEventArgs e)
        {
            OnGridFilterBound(e);
        }

        private void OnGridFilterUnbound(object sender, GridFilterEventArgs e)
        {
            OnGridFilterUnbound(e);
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            base.OnMouseDown(e);
        }

        private void OnMouseEnter(object sender, EventArgs e)
        {
            base.OnMouseEnter(e);
        }

        private void OnMouseHover(object sender, EventArgs e)
        {
            base.OnMouseHover(e);
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            base.OnMouseLeave(e);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            base.OnMouseUp(e);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            base.OnKeyDown(e);
        }

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            base.OnKeyUp(e);
        }

        private void OnPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);
        }

        private void OnDoubleClick(object sender, EventArgs e)
        {
            base.OnDoubleClick(e);
        }

        /// <summary>
        ///     Raises the <see cref="BeforeFiltersChanging" /> event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnBeforeFiltersChanging(EventArgs e)
        {
            if (BeforeFiltersChanging != null)
                BeforeFiltersChanging(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="AfterFiltersChanged" /> event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnAfterFiltersChanged(EventArgs e)
        {
            if (AfterFiltersChanged != null)
                AfterFiltersChanged(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="GridFilterBound" /> event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnGridFilterBound(GridFilterEventArgs e)
        {
            if (GridFilterBound != null)
                GridFilterBound(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="GridFilterUnbound" /> event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnGridFilterUnbound(GridFilterEventArgs e)
        {
            if (GridFilterUnbound != null)
                GridFilterUnbound(this, e);
        }

        /// <summary>
        ///     Cleans up.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        ///     Repositions the grid to match the new size
        /// </summary>
        /// <param name="e">event arguments</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RepositionGrid();
        }
    }
}