using System.Threading.Tasks;

namespace Maze.Modules.Api
{
    public interface IActionResult
    {
        /// <summary>
        ///     Executes the result operation of the action method asynchronously. This method is called by MVC to process
        ///     the result of an action method.
        /// </summary>
        /// <param name="context">
        ///     The context in which the result is executed. The context information includes
        ///     information about the action that was executed and request information.
        /// </param>
        /// <returns>A task that represents the asynchronous execute operation.</returns>
        Task ExecuteResultAsync(ActionContext context);
    }
}