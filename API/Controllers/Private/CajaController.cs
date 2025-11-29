using Data.Repositories;
using Domain;
using Domain.API;
using Domain.Controller.Private.Caja;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers.Private
{
    [Authorize]
    [Route("api/[controller]/[action]")] 
    [ApiController]
    public class CajaController(
        CajaRepository _cajaRepository,
        UsuarioRepository _usuarioRepository,
        CajaBitacoraRepository _cajaBitacoraRepository
    ) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType<EnumModelResponse<short>>(StatusCodes.Status200OK)]
        [ProducesResponseType<BadRequestResponse>(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
              Summary = "Obtener lista de estados",
              Description = "Devuelve una lista de los estados de una caja."
          )]
        public IActionResult GetStatusList()
        {
            var data = CajaEstadoExtensions.GetList();

            return Ok(new EnumModelResponse<short>()
            {
                Data = data,
            });
        }

        [HttpGet]
        [ProducesResponseType<BaseObjectResponse<CajaControllerGetByIdResponse>>(StatusCodes.Status200OK)]
        [SwaggerOperation(
              Summary = "Obtener una caja por su ID.",
              Description = "Devuelve una caja que no este eliminado."
          )]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _cajaRepository.GetById(id);

            if (data == null || data.Eliminado == true)
            {
                return Ok(new BaseObjectResponse<object>
                {
                    Ok = true,
                    Data = null
                });
            }

            return Ok(new BaseObjectResponse<CajaControllerGetByIdResponse>
            {
                Ok = true,
                Data = new CajaControllerGetByIdResponse
                {
                    Id = data.Id,
                    EstadoId = data.EstadoId,
                    Codigo = data.Codigo,
                    Nombre = data.Nombre,
                    Activo = data.Activo,
                    CreadoEn = data.CreadoEn,
                    ActualizadoEn = data.ActualizadoEn,
                }
            });
        }

        [HttpGet]
        [ProducesResponseType<PaginationResponse<CajaControllerGetListResponse>>(StatusCodes.Status200OK)]
        [ProducesResponseType<BadRequestResponse>(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
              Summary = "Obtener lista de cajas",
              Description = "Devuelve una lista de todas las cajas con filtros y paginación."
          )]
        public async Task<IActionResult> GetList([FromQuery] CajaControllerGetListDto param)
        {
            var pagination = await _cajaRepository.GetAll(
                filter: x => !x.Eliminado
                    && (param.EstadoId == null || x.EstadoId.Equals(param.EstadoId))
                    && (param.Codigo == null || x.Codigo.StartsWith(param.Codigo))
                    && (param.Activo == null || x.Activo == param.Activo)
                ,
                ["CajaEstado"],
                selector: x => new CajaControllerGetListResponse
                {
                    Id = x.Id,
                    EstadoId = x.EstadoId,
                    Nombre = x.Nombre,
                    Codigo = x.Codigo,
                    Estado = x.CajaEstado.Nombre,
                    Activo = x.Activo,
                },
                orderBy: x => x.CreadoEn,
                pageArg: param.Page,
                pageSizeArg: param.PageSize
            );

            return Ok(pagination);
        }

        [HttpPost]
        [ProducesResponseType<CajaControllerCreateOneResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<BadRequestResponse>(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "Crear una nueva caja",
            Description = "Crea una nueva caja. Retorna error si ya existe una caja con el mismo código y que no este eliminada."
        )]
        public async Task<IActionResult> CreateOne([FromBody] CajaControllerCreateOneDto data)
        {
            var dbCaja = await _cajaRepository.GetOneByFilter(x => x.Codigo.Equals(data.Codigo) && !x.Eliminado);

            if (dbCaja != null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "Ya existe una caja con el código enviado."
                });
            }

            var newCaja = await _cajaRepository.Create(new()
            {
                EstadoId = CajaEstado.Cerrado.GetValue(),
                Codigo = data.Codigo,
                Nombre = data.Nombre,
                Activo = data.Activo,
                Eliminado = false
            });

            return Ok(new
            {
                newCaja.Id
            });
        }

        [HttpPost]
        [ProducesResponseType<OkResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<BadRequestResponse>(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "Editar una caja",
            Description = "Edita los datos de una caja. La caja no debe de estar eliminada para poder ser editada."
        )]
        public async Task<IActionResult> EditOne(Guid id, [FromBody] CajaControllerEditOneDto data)
        {
            var dbCaja = await _cajaRepository.GetOneByFilter(x => x.Id.Equals(id));

            if (dbCaja == null || dbCaja.Eliminado == true)
            {
                return Ok(new OkResponse());
            }
            else if (dbCaja.EstadoId == CajaEstado.Abierto.GetValue())
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "La caja se encuentra abierta."
                });
            }

            Caja? dbCajaConCodigo = data.Codigo == null ? null : await _cajaRepository.GetOneByFilter(x => x.Codigo.Equals(data.Codigo) && !x.Id.Equals(id));

            if (dbCajaConCodigo != null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "Ya existe una caja con el código enviado."
                });
            }

            dbCaja.Codigo = data.Codigo ?? dbCaja.Codigo;
            dbCaja.Nombre = data.Nombre ?? dbCaja.Nombre;
            dbCaja.Activo = data.Activo ?? dbCaja.Activo;

            await _cajaRepository.Edit(dbCaja);

            return Ok(new OkResponse());
        }

        [HttpPost]
        [ProducesResponseType<OkResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
             Summary = "Eliminar una caja por ID",
             Description = "Busca una caja por su ID y establece su campo 'Eliminado' a 'true'."
         )]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var dbCaja = await _cajaRepository.GetById(id);

            if (dbCaja == null || dbCaja.Eliminado == true)
            {
                return Ok(new OkResponse());
            }

            if (dbCaja.EstadoId == CajaEstado.Abierto.GetValue())
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "La caja se encuentra abierta."
                });
            }

            dbCaja.Eliminado = true;

            await _cajaRepository.Edit(dbCaja);

            return Ok(new OkResponse());
        }

        [HttpPost]
        [ProducesResponseType<OkResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
             Summary = "Desactiva una caja por ID",
             Description = "Busca una caja por su ID y establece su campo 'Activo' a 'false'."
         )]
        public async Task<IActionResult> DeactiveById(Guid id)
        {
            var dbCaja = await _cajaRepository.GetById(id);

            if (dbCaja == null || dbCaja.Activo == false || dbCaja.Eliminado == true)
            {
                return Ok(new OkResponse());
            }

            if (dbCaja.EstadoId == CajaEstado.Abierto.GetValue())
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "La caja se encuentra abierta."
                });
            }

            dbCaja.Activo = false;

            await _cajaRepository.Edit(dbCaja);

            return Ok(new OkResponse());
        }

        [HttpPost]
        [ProducesResponseType<OkResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
             Summary = "Abrir/Aperturar una caja por ID",
             Description = "Apertura una caja para poder realizar facturaciones."
         )]
        public async Task<IActionResult> Open(Guid id, [FromBody] CajaControllerOpenDto data)
        {
            var dbCaja = await _cajaRepository.GetById(id);

            if (dbCaja == null || dbCaja.Activo == false || dbCaja.Eliminado == true)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "La caja no se ha encontrado o se encuentra inactiva."
                });
            }

            if (dbCaja.EstadoId == CajaEstado.Abierto.GetValue())
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "La caja se encuentra abierta."
                });
            }

            var dbUser = await _usuarioRepository.GetById(data.UsuarioId);

            if (dbUser == null || dbUser.Activo == false || dbUser.Eliminado == true)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "El usuario no se ha encontrado o se encuentra inactivo."
                });
            }

            await _cajaRepository.Open(dbUser.Id, dbCaja.Id);

            return Ok(new OkResponse());
        }

        [HttpPost]
        [ProducesResponseType<OkResponse>(StatusCodes.Status200OK)]
        [SwaggerOperation(
             Summary = "Cerrar una caja por ID",
             Description = "Cerrar una caja que ha sido abierta impidiendo que se facture hasta que vuelva a abrirse."
         )]
        public async Task<IActionResult> Close(Guid id, [FromBody] CajaControllerOpenDto data)
        {
            var dbCaja = await _cajaRepository.GetById(id);

            if (dbCaja == null || dbCaja.Activo == false || dbCaja.Eliminado == true)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "La caja no se ha encontrado o se encuentra inactiva."
                });
            }

            if (dbCaja.EstadoId == CajaEstado.Cerrado.GetValue())
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "La caja se encuentra cerrada."
                });
            }

            var dbUser = await _usuarioRepository.GetById(data.UsuarioId);

            if (dbUser == null || dbUser.Activo == false || dbUser.Eliminado == true)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "El usuario no se ha encontrado o se encuentra inactivo."
                });
            }

            var dbCajaBitacora = await _cajaBitacoraRepository.GetOneByFilter(x => x.CajaId.Equals(id) && x.UsuarioId.Equals(data.UsuarioId) && x.FechaCierre == null);

            if (dbCajaBitacora == null)
            {
                return BadRequest(new BadRequestResponse
                {
                    BadMessage = "No existe una vitácora abierta para esta caja o usuario."
                });
            }

            await _cajaRepository.Close(dbUser.Id, dbCaja.Id);

            return Ok(new OkResponse());
        }
    }
}