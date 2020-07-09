using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.TypeDefs;

namespace uk.JohnCook.dotnet.StreamController.Utils
{
    public class OrderSceneItemsByListOfIds : System.Collections.IComparer
    {
        private readonly List<int> collectionOrderList;

        public OrderSceneItemsByListOfIds(List<int> collectionOrderList)
        {
            this.collectionOrderList = collectionOrderList;
        }

        public int Compare(object x, object y)
        {
            if (x == null) { throw new ArgumentNullException(nameof(x)); }
            if (y == null) { throw new ArgumentNullException(nameof(y)); }

            return collectionOrderList.IndexOf((y as ObsSceneItem).Id) - collectionOrderList.IndexOf((x as ObsSceneItem).Id);
        }
    }

    public class OrderByListOfStrings : IComparer<string>
    {
        private readonly List<string> collectionOrderList;

        public OrderByListOfStrings(List<string> collectionOrderList)
        {
            this.collectionOrderList = collectionOrderList;
        }

        public int Compare([AllowNull] string x, [AllowNull] string y)
        {
            return collectionOrderList.IndexOf(y) - collectionOrderList.IndexOf(x);
        }
    }
}
