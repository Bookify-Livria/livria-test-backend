using AutoMapper;
using LivriaBackend.commerce.Domain.Model.Aggregates;
using LivriaBackend.commerce.Domain.Model.Commands;
using LivriaBackend.commerce.Domain.Model.Queries;
using LivriaBackend.commerce.Domain.Model.Services;
using LivriaBackend.commerce.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

namespace LivriaBackend.commerce.Interfaces.REST.Controllers
{
    /// <summary>
    /// Controlador RESTful para gestionar las operaciones relacionadas con las órdenes de compra.
    /// </summary>
    [ApiController]
    [Route("api/v1/orders")]
    [Produces(MediaTypeNames.Application.Json)]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderCommandService _orderCommandService;
        private readonly IOrderQueryService _orderQueryService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="OrdersController"/>.
        /// </summary>
        /// <param name="orderCommandService">El servicio de comandos de órdenes.</param>
        /// <param name="orderQueryService">El servicio de consulta de órdenes.</param>
        /// <param name="mapper">La instancia de AutoMapper para la transformación de objetos.</param>
        public OrdersController(IOrderCommandService orderCommandService, IOrderQueryService orderQueryService, IMapper mapper)
        {
            _orderCommandService = orderCommandService;
            _orderQueryService = orderQueryService;
            _mapper = mapper;
        }

        /// <summary>
        /// Crea una nueva orden de compra en el sistema.
        /// </summary>
        /// <param name="resource">Los datos de la nueva orden a crear.</param>
        /// <returns>
        /// Una acción de resultado HTTP que contiene el <see cref="OrderResource"/> de la orden creada
        /// con un código 201 CreatedAtAction si la operación es exitosa.
        /// Retorna BadRequest (400) si hay un error de argumento o una operación inválida (ej. stock insuficiente).
        /// Retorna StatusCode 500 si ocurre un error inesperado.
        /// </returns>
        [HttpPost]
        [SwaggerOperation(
            Summary= "Crear una nueva orden.",
            Description= "Crea una nueva orden en el sistema."
        )]
        public async Task<ActionResult<OrderResource>> CreateOrder([FromBody] CreateOrderResource resource)
        {
            
            var command = _mapper.Map<CreateOrderCommand>(resource);
            try
            {
                var order = await _orderCommandService.Handle(command);
                var orderResource = _mapper.Map<OrderResource>(order);
                return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, orderResource);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred: " + ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los datos de una orden específica por su identificador único.
        /// </summary>
        /// <param name="id">El identificador único de la orden.</param>
        /// <returns>
        /// Una acción de resultado HTTP que contiene un <see cref="OrderResource"/> si la orden es encontrada (código 200 OK),
        /// o un resultado NotFound (código 404) si la orden no existe.
        /// </returns>
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary= "Obtener los datos de una orden en específico.",
            Description= "Te muestra los datos de la orden que buscaste."
        )]
        public async Task<ActionResult<OrderResource>> GetOrderById(int id)
        {
            var query = new GetOrderByIdQuery(id);
            var order = await _orderQueryService.Handle(query);

            if (order == null)
            {
                return NotFound();
            }

            var orderResource = _mapper.Map<OrderResource>(order);
            return Ok(orderResource);
        }

        /// <summary>
        /// Obtiene los datos de una orden específica por su código de orden.
        /// </summary>
        /// <param name="code">El código alfanumérico de la orden.</param>
        /// <returns>
        /// Una acción de resultado HTTP que contiene un <see cref="OrderResource"/> si la orden es encontrada (código 200 OK),
        /// o un resultado NotFound (código 404) si la orden no existe.
        /// </returns>
        [HttpGet("code/{code}")]
        [SwaggerOperation(
            Summary= "Obtener los datos de una orden en específico por medio de su código.",
            Description= "Te muestra los datos de la orden que buscaste por medio de su código."
        )]
        public async Task<ActionResult<OrderResource>> GetOrderByCode(string code)
        {
            var query = new GetOrderByCodeQuery(code);
            var order = await _orderQueryService.Handle(query);

            if (order == null)
            {
                return NotFound();
            }

            var orderResource = _mapper.Map<OrderResource>(order);
            return Ok(orderResource);
        }

        /// <summary>
        /// Obtiene todas las órdenes realizadas por un cliente de usuario específico.
        /// </summary>
        /// <param name="userClientId">El identificador único del cliente de usuario.</param>
        /// <returns>
        /// Una acción de resultado HTTP que contiene una colección de <see cref="OrderResource"/>
        /// si la operación es exitosa (código 200 OK). Puede ser una colección vacía si el usuario no tiene órdenes.
        /// </returns>
        [HttpGet("users/{userClientId}")]
        [SwaggerOperation(
            Summary= "Obtener los datos de las órdenes de un usuario cliente en específico.",
            Description= "Te muestra los datos de las órdenes del usuario cliente que buscaste."
        )]
        public async Task<ActionResult<IEnumerable<OrderResource>>> GetOrdersByUserId(int userClientId)
        {
            var query = new GetOrdersByUserIdQuery(userClientId);
            var orders = await _orderQueryService.Handle(query);
            var orderResources = _mapper.Map<IEnumerable<OrderResource>>(orders);
            return Ok(orderResources);
        }
    }
}