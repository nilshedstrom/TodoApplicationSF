using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.Http;
using System.Web.Http.Results;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Swashbuckle.Swagger.Annotations;
using TodoActor.Interfaces;
using TodoWebApi.Infrastructure;
using WebApi.Infrastructure;

namespace TodoWebApi.Controllers
{
    /// <summary>
    /// Handles todo-lists
    /// </summary>
    /// <seealso cref="System.Web.Http.ApiController" />
    [ServiceRequestActionFilter]
    public class TodoController : ApiController
    {
        /// <summary>
        /// Gets the todo items for the specified email
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>A list of todo items</returns>
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, "The list of todo items was returned", typeof(IEnumerable<TodoItem>))]
        [SwaggerResponse(HttpStatusCode.NotFound, "Could not find items for specified email")]
        public async Task<IHttpActionResult> GetList([FromUri]string email, 
            CancellationToken cancellationToken)
        {
            ITodoActor todoActor = GetTodoActorProxy(email);
            IEnumerable<TodoActor.Interfaces.TodoItem> list = await todoActor.GetItems(cancellationToken);
            if (list != null)
                return Ok(list.Select(item => new TodoItem
                {
                    Description = item.Description,
                    DateAdded = item.DateAdded,
                    DateFinished = item.DateFinished,
                    Finished = item.Finished
                }));
            return NotFound();
        }

        /// <summary>
        /// Adds a new item to the list
        /// </summary>
        /// <param name="email">The email</param>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Ok if the operation was successfull</returns>
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, "The item was added")]
        public async Task<IHttpActionResult> AddItem([FromUri]string email, 
            AddTodoItemRequest request, CancellationToken cancellationToken)
        {
            ITodoActor todoActor = GetTodoActorProxy(email);
            await todoActor.AddItem(new TodoActor.Interfaces.TodoItem
            {
                Description = request.Description,
                DateAdded = DateTime.Now,
                DateFinished = DateTime.MinValue,
                Finished = false
            }, cancellationToken);
            return Ok();
        }

        private static ITodoActor GetTodoActorProxy(string email)
        {
            return ActorProxy.Create<ITodoActor>(new ActorId(email));
        }
    }


    public class AddTodoItemRequest : IProvideExample
    {
        /// <summary>
        /// The description of the item
        /// </summary>
        public string Description { get; set; }


        public object GetExample()
        {
            return new AddTodoItemRequest
            {
                Description = "New task",
            };
        }
    }

    /// <summary>
    /// Item in a todo-list
    /// </summary>
    /// <seealso cref="IProvideExample" />
    public class TodoItem : IProvideExample
    {
        /// <summary>
        /// The description of the item
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The time when the item was added to the list
        /// </summary>
        public DateTime DateAdded { get; set; }

        /// <summary>
        /// The time when the item was marked as finished
        /// </summary>
        public DateTime DateFinished { get; set; }
        /// <summary>
        ///   <c>true</c> if this item is finished; otherwise, <c>false</c>.
        /// </summary>
        public bool Finished { get; set; }

        public object GetExample()
        {
            return new TodoItem
            {
                Description = "Write Service Fabric demo application",
                DateAdded = DateTime.Now,
                DateFinished = DateTime.MinValue,
                Finished = false
            };
        }
    }
}
