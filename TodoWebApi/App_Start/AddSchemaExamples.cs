using System.Linq;
using TodoWebApi.Infrastructure;

namespace TodoWebApi
{
    public class AddSchemaExamples : Swashbuckle.Swagger.ISchemaFilter
    {
        public void Apply(Swashbuckle.Swagger.Schema schema, Swashbuckle.Swagger.SchemaRegistry schemaRegistry, System.Type type)
        {
            if (type.GetInterfaces().Contains(typeof(IProvideExample)))
            {
                IProvideExample instance = (IProvideExample)System.Activator.CreateInstance(type);
                schema.example = instance.GetExample();
            }
        }
    }
}