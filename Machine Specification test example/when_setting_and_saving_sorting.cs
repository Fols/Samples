using System;
using System.Windows.Forms;
using Machine.Specifications;
using Moq;
using It = Machine.Specifications.It;

namespace Manager.Tests.PrintRelease.PrintReleaseViewPresenterSpecs {
    [Subject(typeof(PrintReleaseViewPresenter))]
    public class when_setting_and_saving_sorting {
        Establish _context = () => {
            _column = new DataGridViewColumn();
            _column.Tag = PrintQueueSortProperty.QueueName;
            _column.CellTemplate = new DataGridViewTextBoxCell();

            _dataGrid = new DataGridView();
            _columnIndex = _dataGrid.Columns.Add(_column);
            _printTeamColumnMapper = Mock.Of<PrintQueueColumnMapper>();
            _gridStateStorage = Mock.Of<GridStateStorage>();
            _gridColumnConfigurator = Mock.Of<Manager.PrintRelease.DataGridColumnsConfigurator.DataGridColumnsConfigurator>();

            _subject = new Builder()
                .WithGridStateStorage(_gridStateStorage)
                .WithPrintTeamMembersColumnMapper(_printTeamColumnMapper)
                .WithDataGridColumnsConfigurator(_gridColumnConfigurator)
                .Build();
        };

        Because of = () =>
            _subject.SetAndSaveSorting(_dataGrid, _columnIndex);

        It should_save_sorting_options = () =>
            Mock.Get(_gridStateStorage)
                .Verify(x => x.SaveSortingOptions(_column, _subject.SortDirection),
                    Times.Once);

        It should_set_glyph_icon_to_header = () =>
            Mock.Get(_gridColumnConfigurator)
                .Verify(x => x.SetGlyphIconToHeader(_dataGrid, _columnIndex, _subject.SortDirection),
                    Times.Once);

        static PrintReleaseViewPresenter _subject;
        static Int32 _columnIndex;
        static DataGridViewColumn _column;
        static PrintQueueColumnMapper _printTeamColumnMapper;
        static Manager.PrintRelease.DataGridColumnsConfigurator.DataGridColumnsConfigurator _gridColumnConfigurator;
        static GridStateStorage _gridStateStorage;
        static DataGridView _dataGrid;
    }
}