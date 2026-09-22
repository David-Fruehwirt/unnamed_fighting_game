namespace PixelAuthor;

public static partial class Program
{
    // Weighted median-cut with explicit ordering and nearest-palette assignment; no dithering.
    static byte[] QuantizeStage(byte[] rgba)
    {
        var histogram=new SortedDictionary<int,int>();
        for(int i=0;i<rgba.Length;i+=4)if(rgba[i+3]!=0)
        {
            int rgb=(rgba[i]<<16)|(rgba[i+1]<<8)|rgba[i+2];
            histogram[rgb]=histogram.GetValueOrDefault(rgb)+1;
        }
        int Channel(int color,int channel)=>(color>>(16-channel*8))&255;
        int Range(List<int> box,int channel)=>box.Max(c=>Channel(c,channel))-box.Min(c=>Channel(c,channel));
        var boxes=new List<List<int>>{histogram.Keys.ToList()};
        while(boxes.Count<32)
        {
            int index=Enumerable.Range(0,boxes.Count).Where(i=>boxes[i].Count>1)
                .OrderByDescending(i=>Enumerable.Range(0,3).Max(c=>Range(boxes[i],c)))
                .ThenByDescending(i=>boxes[i].Sum(c=>histogram[c])).ThenBy(i=>i).First();
            var box=boxes[index];int axis=Enumerable.Range(0,3).OrderByDescending(c=>Range(box,c)).ThenBy(c=>c).First();
            box=box.OrderBy(c=>Channel(c,axis)).ThenBy(c=>c).ToList();
            int half=(box.Sum(c=>histogram[c])+1)/2,total=0,cut=0;
            do{total+=histogram[box[cut++]];}while(total<half&&cut<box.Count-1);
            cut=Math.Min(cut,box.Count-1);
            boxes[index]=box.Take(cut).ToList();boxes.Add(box.Skip(cut).ToList());
        }
        var palette=boxes.Select(box=>{
            long count=box.Sum(c=>(long)histogram[c]);
            return Enumerable.Range(0,3).Select(channel=>(int)((box.Sum(c=>(long)Channel(c,channel)*histogram[c])+count/2)/count)).ToArray();
        }).ToArray();
        var map=histogram.Keys.ToDictionary(c=>c,c=>palette.Select((p,i)=>(p,i))
            .OrderBy(v=>Enumerable.Range(0,3).Sum(channel=>Math.Pow(Channel(c,channel)-v.p[channel],2)))
            .ThenBy(v=>v.i).First().p);
        for(int i=0;i<rgba.Length;i+=4)if(rgba[i+3]!=0)
        {
            var p=map[(rgba[i]<<16)|(rgba[i+1]<<8)|rgba[i+2]];
            for(int c=0;c<3;c++)rgba[i+c]=(byte)p[c];
        }
        return rgba;
    }
}
