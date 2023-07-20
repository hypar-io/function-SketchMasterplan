using Elements.Serialization.JSON;
using Newtonsoft.Json;

namespace Elements
{
    public class DeferredElement : Element
    {
        public string Json { get; set; }
        public string EncodedElementType { get; set; }
        public DeferredElement(Element e) : base()
        {
            EncodedElementType = e.GetType().ToString();
            JsonInheritanceConverter.ElementwiseSerialization = true;
            Json = JsonConvert.SerializeObject(e);
            JsonInheritanceConverter.ElementwiseSerialization = false;
        }
    }
}