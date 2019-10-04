using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Manager.Locations {
    public class LocationsPresenter : ManagerPlugin {
        private readonly ILocationsView _view;
        private readonly ImmutableList<ISearchProvider> _searchProviders;
        private readonly UnitOfWorkFactory2 _unitOfWorkFactory;

        public LocationsPresenter() { }

        public LocationsPresenter (ILocationsView view, LocationsSearchProvider locationsSearchProvider, UnitOfWorkFactory2 unitOfWorkFactory) {
            if (view == null) {
                throw new ArgumentNullException(nameof(view));
            }
            if (locationsSearchProvider == null) {
                throw new ArgumentNullException(nameof(locationsSearchProvider));
            }
            if (unitOfWorkFactory == null) {
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
            }

            _searchProviders = ImmutableList.Create((ISearchProvider)locationsSearchProvider);
            _unitOfWorkFactory = unitOfWorkFactory;
            view.InitializePresenter(this);
            _view = view;
        }

        public override string Name {
            get { return Resources.LocationPlugin; }
        }

        public override String ID {
            get { return PluginIds.Locations; }
        }

        // TODO: I assume this is going away during the refactoring so I hard-coded a cast for now (meaning it's not testable).
        public override UserControl CreateView() {
            return (LocationsView)_view;
        }

        public override Task UnloadViewData() {
            return Task.CompletedTask;
        }

        public override Task LoadViewData() {
            _view.ShowLocations();
            return Task.CompletedTask;
        }

        public override ImmutableList<ISearchProvider> SearchProviders {
            get {
                return _searchProviders;
            }
        }

        public void ShowLocationWithId(Int32 locationId) {
            _unitOfWorkFactory.ExecuteInsideTransaction(uow => {
                var location = uow.Repositories.Locations.WithId(locationId);
                if (!location.HasValue) {
                    _view.ShowLocations();
                    return;
                }

                _view.ShowLocationDetails(location.Value);
            });
        }
    }
}
