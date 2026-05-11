using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoBlazor;

public class Department
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("DEPTNO")]
    public int DeptNo { get; set; }

    [BsonElement("DNAME")]
    public string DName { get; set; } = string.Empty;

    [BsonElement("LOC")]
    public string Loc { get; set; } = string.Empty;
}