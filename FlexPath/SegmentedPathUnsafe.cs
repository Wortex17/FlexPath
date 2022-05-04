using System;

namespace FlexPath
{
    /**
     * Represents an encoded array of path segments.
     * Path segments are separated by a fixed separator.
     * This type is unsafe, as pushing a segment containing the separator will not be caught and might break the array.
     */
    struct SegmentedPathUnsafe
    {
        public string Segments { get; private set; }
        public int Count { get; private set; }

        public void Push(string segment)
        {
            Count++;
            if (String.IsNullOrEmpty(Segments))
            {
                Segments = segment;
            }
            else
            {
                Segments += "/" + segment;
            }
        }

        public void Pop()
        {
            Count--;
            for (int i = Segments.Length-1; i >= 0; i--)
            {
                if (Segments[i] == '/')
                {
                    Segments = Segments.Remove(i);
                    return;
                }
            }
            Segments = null;
        }
        
        public void Clear()
        {
            Count = 0;
            Segments = null;
        }

        public void IterateSegments(Action<String> segmentAction)
        {
            if(Segments == null)
                return;
            
            var segments = Segments.Split('/');
            foreach (var segment in segments)
            {
                segmentAction(segment);
            }
        }
    }
}