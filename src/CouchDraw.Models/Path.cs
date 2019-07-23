using System.Collections.Generic;

namespace CouchDraw.Models
{
    public class Path
    {
        public string Type => "path";
        public string Id { get; set; }
        public string Color { get; set; } = "#000000";
        public string CreatedBy { get; set; }
        public List<Point> Points { get; set; } = new List<Point>();

        public Path(string id)
        {
            Id = id;
        }
    }
}
