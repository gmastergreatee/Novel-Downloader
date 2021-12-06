namespace EpubGenerator
{
    public class ChapterData
    {
        public int VolumeId { get; set; } = 0;
        public string VolumeName { get; set; } = "";
        public string ChapterName { get; set; } = "";
        public string ChapterContent { get; set; } = "";
        public bool ContainsScript { get; set; } = false;
    }
}
