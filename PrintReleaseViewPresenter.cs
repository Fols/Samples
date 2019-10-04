using System;
using System.Collections.Immutable;
using System.Windows.Forms;

namespace Manager.PrintRelease {
    public class PrintReleaseViewPresenter {
        private readonly GridStateStorage _gridStateStorage;
        private readonly DataGridColumnsConfigurator.DataGridColumnsConfigurator _dataGridColumnsConfigurator;
        private readonly PrintQueueColumnMapper _printQueueColumnMapper;
        private readonly PrintQueueVmListBuilder _printQueueVmListBuilder;
        private readonly UnitOfWorkFactory2 _unitOfWorkFactory;

        private SortDirection _sortDirection = SortDirection.Ascending;
        private PrintQueueSortProperty _sortProperty;

        public PrintReleaseViewPresenter(
            UnitOfWorkFactory2 unitOfWorkFactory,
            GridStateStorage gridStateStorage,
            DataGridColumnsConfigurator.DataGridColumnsConfigurator dataGridColumnsConfigurator,
            PrintQueueColumnMapper printQueueColumnMapper,
            PrintQueueVmListBuilder printQueueVmListBuilder) {

            if (unitOfWorkFactory == null) {
                throw new ArgumentNullException(nameof (unitOfWorkFactory));
            }

            if (gridStateStorage == null) {
                throw new ArgumentNullException(nameof (gridStateStorage));
            }

            if (dataGridColumnsConfigurator == null) {
                throw new ArgumentNullException(nameof (dataGridColumnsConfigurator));
            }

            if (printQueueColumnMapper == null) {
                throw new ArgumentNullException(nameof (printQueueColumnMapper));
            }

            if (printQueueVmListBuilder == null) {
                throw new ArgumentNullException(nameof (printQueueVmListBuilder));
            }

            _unitOfWorkFactory = unitOfWorkFactory;
            _gridStateStorage = gridStateStorage;
            _dataGridColumnsConfigurator = dataGridColumnsConfigurator;
            _printQueueColumnMapper = printQueueColumnMapper;
            _printQueueVmListBuilder = printQueueVmListBuilder;
        }

        public SortDirection SortDirection {
            get { return _sortDirection; }
            set { _sortDirection = value; }
        }

        public (Int32 sortedColumnIndex, SortDirection sortDirection)? LoadSortingOptions(DataGridView dataGrid) {
            if (dataGrid == null) {
                throw new ArgumentNullException(nameof (dataGrid));
            }

            return _gridStateStorage.LoadSortingOptions(dataGrid);
        }

        public void SetSorting(
            String columnName,
            SortDirection sortDirection,
            DataGridView dataGrid,
            Int32 columnIndex) {
            if (columnName == null) {
                throw new ArgumentNullException(nameof (columnName));
            }
            if (dataGrid == null) {
                throw new ArgumentNullException(nameof (dataGrid));
            }

            var columnSortProperty = GetPrintTeamMembersSortPropertyByColumnName(columnName);
            SetSortBy(columnSortProperty);
            _sortDirection = sortDirection;
            SetGlyphIconToHeader(
                dataGrid,
                columnIndex);
        }

        public PrintQueueSortProperty GetPrintTeamMembersSortPropertyByColumnName(String columnName) {
            if (columnName == null) {
                throw new ArgumentNullException(nameof (columnName));
            }

            return _printQueueColumnMapper.GetPrintTeamMembersSortPropertyByColumn(columnName);
        }

        public void FillDataGrid(DataGridView dataGrid, Guid? selectedTeamId) {
            dataGrid.Rows.Clear();

            if (selectedTeamId.HasValue) {
                var printTeamMembers = GetPrintTeamMembers(selectedTeamId.Value);

                foreach (var printQueueVm in printTeamMembers) {
                    var index = dataGrid.Rows.Add();
                    dataGrid.Rows[index].Tag = printQueueVm;
                }
            }
        }

        public void FillLastVisibleColumn(DataGridView dataGrid) {
            if (dataGrid == null) {
                throw new ArgumentNullException(nameof (dataGrid));
            }

            _dataGridColumnsConfigurator.FillLastVisibleColumn(dataGrid);
        }

        public void SnapColumn(DataGridView dataGrid, Int32 columnIndex) {
            if (dataGrid == null) {
                throw new ArgumentNullException(nameof (dataGrid));
            }

            _gridStateStorage.SnapColumn(dataGrid,
                columnIndex);
        }

        public void SetAndSaveSorting(DataGridView dataGrid, Int32 columnIndex) {
            var sortingColumn = dataGrid.Columns[columnIndex];

            if (sortingColumn.Tag == null) {
                return;
            }

            var columnSortProperty = (PrintQueueSortProperty)sortingColumn.Tag;

            SetSortBy(columnSortProperty);
            SaveSortingOptions(sortingColumn);
            SetGlyphIconToHeader(dataGrid, columnIndex);
        }

        public Boolean ConfigureColumns(DataGridView dataGrid) {
            if (dataGrid == null) {
                throw new ArgumentNullException(nameof (dataGrid));
            }

            return _dataGridColumnsConfigurator.ConfigureColumns(dataGrid);
        }

        public void SaveGridColumns(DataGridView dataGrid) {
            if (dataGrid == null) {
                throw new ArgumentNullException(nameof (dataGrid));
            }

            _gridStateStorage.SaveGridColumns(dataGrid);
        }

        private void SetSortBy(PrintQueueSortProperty columnSortProperty) {
            if (_sortProperty != columnSortProperty) {
                _sortProperty = columnSortProperty;
                SortDirection = SortDirection.Ascending;
            } else {
                // Same column clicked, just swap sort order
                SortDirection = SortDirection == SortDirection.Ascending
                    ? SortDirection.Descending
                    : SortDirection.Ascending;
            }
        }

        private void SaveSortingOptions(DataGridViewColumn column) {
            if (column == null) {
                throw new ArgumentNullException(nameof (column));
            }

            _gridStateStorage.SaveSortingOptions(column,
                SortDirection);
        }

        private void SetGlyphIconToHeader(DataGridView dataGrid, Int32 sortColumnIndex) {
            if (dataGrid == null) {
                throw new ArgumentNullException(nameof (dataGrid));
            }

            _dataGridColumnsConfigurator.SetGlyphIconToHeader(
                dataGrid,
                sortColumnIndex,
                SortDirection);
        }

        private ImmutableList<PrintQueueVm.PrintQueueVm> GetPrintTeamMembers(Guid teamId) {
            return _unitOfWorkFactory.ExecuteInsideTransaction(uow => {
                var teamPrintQueues = uow.Repositories.PrintQueues.InTeam(teamId);

                return _printQueueVmListBuilder.BuildSortedList(
                    teamPrintQueues,
                    uow.Repositories.PrinterServices,
                    _sortDirection,
                    _sortProperty);
            });
        }
    }
}