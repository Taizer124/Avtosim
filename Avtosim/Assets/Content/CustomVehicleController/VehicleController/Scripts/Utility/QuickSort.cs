using System.Collections.Generic;

namespace Assets.VehicleController
{
    public class QuickSort
    {
        public static void Sort(List<RacerProgress> arr, int left, int right)
        {
            if (left < right)
            {
                int pivot = Partition(arr, left, right);

                Sort(arr, left, pivot - 1);
                Sort(arr, pivot + 1, right);
            }
        }

        private static int Partition(List<RacerProgress> arr, int left, int right)
        {
            float pivot = arr[right].RacerProgressNormalized + arr[right].LapsPassed;
            int i = left - 1;

            for (int j = left; j < right; j++)
            {
                if (arr[j].RacerProgressNormalized + arr[j].LapsPassed > pivot)
                {
                    i++;
                    var temp = arr[i];
                    arr[i] = arr[j];
                    arr[j] = temp;
                }
            }

            var temp1 = arr[i + 1];
            arr[i + 1] = arr[right];
            arr[right] = temp1;

            return i + 1;
        }
    }
}
