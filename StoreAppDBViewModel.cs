using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace bajanetLauncher {
    public class StoreAppDBViewModel {
        public ObservableCollection<StoreApp> Items { get; }

        public StoreAppDBViewModel(IEnumerable<StoreApp> items) {
            Items = new ObservableCollection<StoreApp>(items);
        } 
    }
}