using System.Collections.Generic;
using TodoWebApi.Infrastructure;

namespace WebApi.Infrastructure
{
    /// <summary>
    /// Describes the error state
    /// </summary>
    public class ErrorResponse : IProvideExample
    {
        /// <summary>
        /// The error message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// A dictionary with the name of the parameter and its error messages
        /// </summary>
        public Dictionary<string, string[]> ModelState { get; set; }

        public object GetExample()
        {
            return new ErrorResponse
            {
                Message = "The request is invalid.",
                ModelState = new Dictionary<string, string[]>
                {
                    {"request.TrafficInsurancePrice", new[] {"'Traffic Insurance Price' must be less than '200'."}}
                }
            };
        }
    }
}