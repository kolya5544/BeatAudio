using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Utils
{
    public static void RemoveNthBeats(ref List<Beat> beats, int[] toRemove, int beatSampleSize)
    {
        for (int i = 0; i < beats.Count; i++)
        {
            Beat b = beats[i];
            if (toRemove.Contains(b.position % beatSampleSize))
            {
                beats.Remove(b);
                i--; continue;
            }
        }
        FixPositionOrder(ref beats);
    }

    public static void SwapBeats(ref List<Beat> beats, int swapN1, int swapN2, int beatSampleSize)
    {
        for (int i = 0; i < (beats.Count / beatSampleSize) + (beats.Count % beatSampleSize == 0 ? 0 : 1); i++)
        {
            var batch = beats.GetRange(i * beatSampleSize, Math.Min(beatSampleSize, beats.Count - i * beatSampleSize));

            if (swapN1 < batch.Count && swapN2 < batch.Count)
            {
                var l = batch[swapN1].position;
                batch[swapN1].position = batch[swapN2].position;
                batch[swapN2].position = l;
            }
        }
        beats = Reorder(beats);
    }

    public static void ReverseBeats(ref List<Beat> beats, int beatSampleSize = 1)
    {
        var margin = (beats.Count / beatSampleSize) + (beats.Count % beatSampleSize == 0 ? 0 : 1);
        List<Beat> output = new(beats.Count);
        for (int i = margin - 1; i >= 0; i--)
        {
            var batch = beats.GetRange(i * beatSampleSize, Math.Min(beatSampleSize, beats.Count - i * beatSampleSize));

            output.AddRange(batch);
        }
        FixPositionOrder(ref output);
        beats = output;
    }

    public static void DoubleBeats(ref List<Beat> beats, int[] doubleList, int beatSampleSize)
    {
        var limit = (beats.Count / beatSampleSize) + (beats.Count % beatSampleSize == 0 ? 0 : 1);
        doubleList = doubleList.OrderBy((z) => z).ToArray();
        List<Beat> output = new(beats.ToArray());
        int pers = 0;
        for (int i = 0; i < limit; i++)
        {
            var batch = beats.GetRange(i * beatSampleSize, Math.Min(beatSampleSize, beats.Count - i * beatSampleSize));

            for (int z = 0; z < doubleList.Length; z++)
            {
                int perm = doubleList[z];
                if (perm < batch.Count)
                {
                    output.Insert(perm + pers + i * beatSampleSize + 1, batch[perm]);
                    pers += 1;
                }
            }
        }
        FixPositionOrder(ref output);
        beats = output;
    }

    public static List<Beat> Reorder(List<Beat> beats)
    {
        beats = beats.OrderBy((z) => z.position).ToList();
        return beats;
    }

    public static void FixPositionOrder(ref List<Beat> beats)
    {
        for (int i = 0; i < beats.Count; i++)
        {
            beats[i].position = i;
        }
    }
}
