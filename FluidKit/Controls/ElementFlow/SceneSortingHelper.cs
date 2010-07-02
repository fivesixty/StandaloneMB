using System;
using System.Windows;
using System.Collections;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FluidKit.Controls
{
    public static class SceneSortingHelper
    {
        /// <summary>
        /// Sort Modelgroups in Farthest to Closest order, to enable transparency
        /// Should be applied whenever the scene is significantly re-oriented
        /// </summary>
        public static void AlphaSort(Point3D CameraPosition, Visual3DCollection Models)
        {
            ArrayList list = new ArrayList();
            foreach (Visual3D model in Models)
            {
                double distance = (Point3D.Subtract(CameraPosition, ((ModelUIElement3D)model).Model.Bounds.Location )).Length;
                list.Add(new ModelDistance(distance, model));
            }
            list.Sort(new DistanceComparer(SortDirection.FarToNear));
            Models.Clear();
            foreach (ModelDistance modelDistance in list)
            {
                Models.Add(modelDistance.model);
            }
        }

        private class ModelDistance
        {
            public ModelDistance(double distance, Visual3D model)
            {
                this.distance = distance;
                this.model = model;
            }

            public double distance;
            public Visual3D model;
        }

        private enum SortDirection
        {
            NearToFar,
            FarToNear
        }

        private class DistanceComparer : IComparer
        {
            public DistanceComparer(SortDirection sortDirection)
            {
                _sortDirection = sortDirection;
            }

            int IComparer.Compare(Object o1, Object o2)
            {
                double x1 = ((ModelDistance)o1).distance;
                double x2 = ((ModelDistance)o2).distance;
                if (_sortDirection == SortDirection.NearToFar)
                {
                    return (int)(x1 - x2);
                }
                else
                {
                    return (int)(-(x1 - x2));
                }
            }

            private SortDirection _sortDirection;
        }

    }

}
