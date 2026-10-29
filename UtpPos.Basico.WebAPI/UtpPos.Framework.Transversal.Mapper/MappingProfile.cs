using AutoMapper;
using UtpPos.Framework.Application.DTO.Business;
using UtpPos.Framework.Application.DTO.Class;
using UtpPos.Framework.Application.DTO.Parameter;
using UtpPos.Framework.Application.DTO.Rbac;
using UtpPos.Framework.Application.DTO.User;
using UtpPos.Framework.Application.DTO.Product;
using UtpPos.Framework.Application.DTO.Inventory;
using UtpPos.Framework.Application.DTO.Kardex;
using UtpPos.Framework.Application.DTO.Purchase;
using UtpPos.Framework.Application.DTO.Sale;
using UtpPos.Framework.Application.DTO.ThirdParty;
using DomainBusiness = UtpPos.Framework.Domain.Entity.Business.Business;
using DomainClass = UtpPos.Framework.Domain.Entity.Class.Class;
using DomainParameter = UtpPos.Framework.Domain.Entity.Parameter.Parameter;
using DomainUser = UtpPos.Framework.Domain.Entity.User.User;
using DomainProduct = UtpPos.Framework.Domain.Entity.Product.Product;
using DomainProductFilter = UtpPos.Framework.Domain.Entity.Product.ProductFilter;
using DomainProductUpdate = UtpPos.Framework.Domain.Entity.Product.ProductUpdate;
using DomainInventory = UtpPos.Framework.Domain.Entity.Inventory.Inventory;
using DomainInventoryFilter = UtpPos.Framework.Domain.Entity.Inventory.InventoryFilter;
using DomainInventoryMovement = UtpPos.Framework.Domain.Entity.Inventory.InventoryMovement;
using DomainInventoryMovementResult = UtpPos.Framework.Domain.Entity.Inventory.InventoryMovementResult;
using DomainKardex = UtpPos.Framework.Domain.Entity.Kardex.Kardex;
using DomainKardexFilter = UtpPos.Framework.Domain.Entity.Kardex.KardexFilter;
using DomainPurchase = UtpPos.Framework.Domain.Entity.Purchase.Purchase;
using DomainPurchaseCancel = UtpPos.Framework.Domain.Entity.Purchase.PurchaseCancel;
using DomainPurchaseCancelResult = UtpPos.Framework.Domain.Entity.Purchase.PurchaseCancelResult;
using DomainPurchaseCreate = UtpPos.Framework.Domain.Entity.Purchase.PurchaseCreate;
using DomainPurchaseCreateDetail = UtpPos.Framework.Domain.Entity.Purchase.PurchaseCreateDetail;
using DomainPurchaseCreatePayment = UtpPos.Framework.Domain.Entity.Purchase.PurchaseCreatePayment;
using DomainPurchaseDetail = UtpPos.Framework.Domain.Entity.Purchase.PurchaseDetail;
using DomainPurchaseFilter = UtpPos.Framework.Domain.Entity.Purchase.PurchaseFilter;
using DomainPurchasePayment = UtpPos.Framework.Domain.Entity.Purchase.PurchasePayment;
using DomainPurchaseWriteResult = UtpPos.Framework.Domain.Entity.Purchase.PurchaseWriteResult;
using DomainSale = UtpPos.Framework.Domain.Entity.Sale.Sale;
using DomainSaleCancel = UtpPos.Framework.Domain.Entity.Sale.SaleCancel;
using DomainSaleCancelResult = UtpPos.Framework.Domain.Entity.Sale.SaleCancelResult;
using DomainSaleCreate = UtpPos.Framework.Domain.Entity.Sale.SaleCreate;
using DomainSaleCreateDetail = UtpPos.Framework.Domain.Entity.Sale.SaleCreateDetail;
using DomainSaleCreatePayment = UtpPos.Framework.Domain.Entity.Sale.SaleCreatePayment;
using DomainSaleDetail = UtpPos.Framework.Domain.Entity.Sale.SaleDetail;
using DomainSaleFilter = UtpPos.Framework.Domain.Entity.Sale.SaleFilter;
using DomainSalePayment = UtpPos.Framework.Domain.Entity.Sale.SalePayment;
using DomainSaleWriteResult = UtpPos.Framework.Domain.Entity.Sale.SaleWriteResult;
using DomainThirdParty = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdParty;
using DomainThirdPartyBusiness = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyBusiness;
using DomainThirdPartyBusinessFilter = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyBusinessFilter;
using DomainThirdPartyBusinessResult = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyBusinessResult;
using DomainThirdPartyCreateForBusiness = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyCreateForBusiness;
using DomainThirdPartyCreate = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyCreate;
using DomainThirdPartyFilter = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyFilter;
using DomainThirdPartyResult = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyResult;
using DomainThirdPartyUpdate = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyUpdate;
using UtpPos.Framework.Domain.Entity.Rbac;

namespace UtpPos.Framework.Transversal.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region Map Business
            CreateMap<DomainBusiness, BusinessDto>().ReverseMap();
            CreateMap<BusinessCreateDto, DomainBusiness>();
            CreateMap<BusinessUpdateDto, DomainBusiness>()
                .ForMember(destination => destination.Id, source => source.Ignore());
            #endregion

            #region Map Class
            CreateMap<DomainClass, ClassDto>().ReverseMap();
            CreateMap<ClassCreateDto, DomainClass>();
            CreateMap<ClassUpdateDto, DomainClass>()
                .ForMember(destination => destination.Id, source => source.Ignore());
            #endregion

            #region Map Parameter
            CreateMap<DomainParameter, ParameterDto>().ReverseMap();
            CreateMap<ParameterCreateDto, DomainParameter>();
            CreateMap<ParameterUpdateDto, DomainParameter>()
                .ForMember(destination => destination.Id, source => source.Ignore());
            #endregion

            #region Map Rbac
            CreateMap<RbacModule, RbacModuleDto>().ReverseMap();
            CreateMap<RbacModuleCreateDto, RbacModule>();
            CreateMap<RbacAction, RbacActionDto>().ReverseMap();
            CreateMap<RbacActionCreateDto, RbacAction>();
            CreateMap<RbacPermission, RbacPermissionDto>().ReverseMap();
            CreateMap<RbacPermissionCreateDto, RbacPermission>();
            CreateMap<RbacRole, RbacRoleDto>().ReverseMap();
            CreateMap<RbacRoleCreateDto, RbacRole>();
            CreateMap<RbacRoleUpdateDto, RbacRole>()
                .ForMember(destination => destination.Id, source => source.MapFrom(value => value.RoleId));
            CreateMap<RbacRolePermission, RbacRolePermissionDto>().ReverseMap();
            CreateMap<RbacUserRole, RbacUserRoleDto>().ReverseMap();
            CreateMap<RbacUserPermission, RbacUserPermissionDto>();
            #endregion

            #region Map User
            CreateMap<DomainUser, UserDto>();
            CreateMap<UserCreateDto, DomainUser>();
            #endregion

            #region Map Product
            CreateMap<DomainProduct, ProductDto>(); // BD -> Domain -> DTO -> Frontend
            CreateMap<ProductCreateDto, DomainProduct>() // Frontend -> DTO -> Domain -> BD
                .ForMember(destination => destination.SaleValue, source => source.Ignore());
            CreateMap<ProductUpdateDto, DomainProductUpdate>() // Frontend -> DTO -> Domain -> BD
                .ForMember(destination => destination.Id, source => source.Ignore()); // Ignore the Id property during mapping
            CreateMap<ProductListFilterDto, DomainProductFilter>(); // Frontend -> DTO -> Domain -> BD
            #endregion

            #region Map Inventory
            CreateMap<DomainInventory, InventoryDto>();
            CreateMap<InventoryListFilterDto, DomainInventoryFilter>();
            CreateMap<InventoryMovementCreateDto, DomainInventoryMovement>();
            CreateMap<DomainInventoryMovementResult, InventoryMovementResultDto>();
            #endregion

            #region Map Kardex
            CreateMap<DomainKardex, KardexDto>();
            CreateMap<KardexListFilterDto, DomainKardexFilter>();
            #endregion

            #region Map Purchase
            CreateMap<DomainPurchase, PurchaseDto>();
            CreateMap<DomainPurchase, PurchaseDetailDto>();
            CreateMap<DomainPurchaseDetail, PurchaseLineDto>();
            CreateMap<DomainPurchasePayment, PurchasePaymentDto>();
            CreateMap<PurchaseListFilterDto, DomainPurchaseFilter>();
            CreateMap<PurchaseCreateDto, DomainPurchaseCreate>();
            CreateMap<PurchaseCreateDetailDto, DomainPurchaseCreateDetail>();
            CreateMap<PurchaseCreatePaymentDto, DomainPurchaseCreatePayment>();
            CreateMap<PurchaseCancelDto, DomainPurchaseCancel>()
                .ForMember(destination => destination.PurchaseId, source => source.Ignore())
                .ForMember(destination => destination.UserId, source => source.Ignore());
            CreateMap<DomainPurchaseWriteResult, PurchaseWriteResultDto>();
            CreateMap<DomainPurchaseCancelResult, PurchaseCancelResultDto>();
            #endregion

            #region Map Sale
            CreateMap<DomainSale, SaleDto>();
            CreateMap<DomainSale, SaleDetailDto>();
            CreateMap<DomainSaleDetail, SaleLineDto>();
            CreateMap<DomainSalePayment, SalePaymentDto>();
            CreateMap<SaleListFilterDto, DomainSaleFilter>();
            CreateMap<SaleCreateDto, DomainSaleCreate>();
            CreateMap<SaleCreateDetailDto, DomainSaleCreateDetail>();
            CreateMap<SaleCreatePaymentDto, DomainSaleCreatePayment>();
            CreateMap<SaleCancelDto, DomainSaleCancel>()
                .ForMember(destination => destination.SaleId, source => source.Ignore())
                .ForMember(destination => destination.UserId, source => source.Ignore());
            CreateMap<DomainSaleWriteResult, SaleWriteResultDto>();
            CreateMap<DomainSaleCancelResult, SaleCancelResultDto>();
            #endregion

            #region Map ThirdParty
            CreateMap<DomainThirdParty, ThirdPartyDto>();
            CreateMap<DomainThirdPartyBusiness, ThirdPartyBusinessDto>();
            CreateMap<ThirdPartyListFilterDto, DomainThirdPartyFilter>();
            CreateMap<ThirdPartyBusinessListFilterDto, DomainThirdPartyBusinessFilter>();
            CreateMap<ThirdPartyCreateDto, DomainThirdPartyCreate>()
                .ForMember(destination => destination.CreatedByUserId, source => source.Ignore());
            CreateMap<ThirdPartyCreateForBusinessDto, DomainThirdPartyCreateForBusiness>()
                .ForMember(destination => destination.CreatedByUserId, source => source.Ignore());
            CreateMap<ThirdPartyUpdateDto, DomainThirdPartyUpdate>()
                .ForMember(destination => destination.Id, source => source.Ignore());
            CreateMap<DomainThirdPartyResult, ThirdPartyResultDto>();
            CreateMap<DomainThirdPartyBusinessResult, ThirdPartyBusinessResultDto>();
            #endregion

            //CreateMap<ClsMystery_Ganador, ClsMystery_Ganador>().ReverseMap();

            //CreateMap<ClsUsuarioAppDTO, TBL_USUARIOS>().ReverseMap()
            //        .ForMember(destination => destination.ID, source => source.MapFrom(src => src.USU_ID))
            //        .ForMember(destination => destination.NIT, source => source.MapFrom(src => src.USU_NIT))
            //        .ForMember(destination => destination.NOMBRE, source => source.MapFrom(src => src.USU_NOMBRE))
            //        .ForMember(destination => destination.CORREO, source => source.MapFrom(src => src.USU_CORREO))
            //        .ForMember(destination => destination.TIPO, source => source.MapFrom(src => src.USU_TIPO))
            //        .ForMember(destination => destination.TIPO_APP, source => source.MapFrom(src => src.USU_TIPO_APP))
            //        .ForMember(destination => destination.CELULAR, source => source.MapFrom(src => src.USU_CELULAR))
            //        .ForMember(destination => destination.FILTRAR_FECHAS, source => source.MapFrom(src => src.USU_FILTRAR_FECHAS));

            //CreateMap<ClsUsuarioAppDTO, TBL_TERCEROS>().ReverseMap()
            //        .ForMember(destination => destination.ID, source => source.MapFrom(src => src.TER_ID))
            //        .ForMember(destination => destination.NIT, source => source.MapFrom(src => src.TER_NIT))
            //        .ForMember(destination => destination.NOMBRE, source => source.MapFrom(src => src.TER_NOMBRE_COMPLETO))
            //        .ForMember(destination => destination.CORREO, source => source.MapFrom(src => src.TER_CORREO))
            //        .ForMember(destination => destination.CELULAR, source => source.MapFrom(src => src.TER_TELEFONO_CELULAR));

            //CreateMap<ClsUsuarioApp_AccesoDTO, TBL_USUARIOS_ACCESO>().ReverseMap()
            //    .ForMember(destination => destination.ID, source => source.MapFrom(src => src.UA_ID))
            //    .ForMember(destination => destination.LOGIN, source => source.MapFrom(src => src.UA_LOGIN))
            //    .ForMember(destination => destination.PASSWORD, source => source.MapFrom(src => src.UA_PASSWORD))
            //    .ForMember(destination => destination.PASSWORD_VENCE, source => source.MapFrom(src => src.UA_PASSWORD_VENCE))
            //    .ForMember(destination => destination.PASSWORD_PROXIMO_CAMBIO, source => source.MapFrom(src => src.UA_PASSWORD_PROXIMO_CAMBIO));

            //CreateMap<ClsUsuarioApp_PermisosDTO, TBL_PERMISOS>().ReverseMap()
            //.ForMember(destination => destination.ID, source => source.MapFrom(src => src.PER_NEGOCIO_ID))
            //.ForMember(destination => destination.EMPRESA_ID, source => source.MapFrom(src => src.NEGO_EMPRESA_ID))
            //.ForMember(destination => destination.NIT, source => source.MapFrom(src => src.NEGO_NIT))
            //.ForMember(destination => destination.NOMBRE, source => source.MapFrom(src => src.NEGO_NOMBRE))
            //.ForMember(destination => destination.DIRECCION, source => source.MapFrom(src => src.NEGO_DIRECCION))
            //.ForMember(destination => destination.TELEFONO, source => source.MapFrom(src => src.NEGO_TELEFONO))
            //.ForMember(destination => destination.CANTIDAD_DECIMAL, source => source.MapFrom(src => src.NEGO_CANTIDAD_DECIMAL))
            //.ForMember(destination => destination.MONEDA_ID, source => source.MapFrom(src => src.NEGO_MONEDA_ID))
            //.ForMember(destination => destination.MONEDA_DESCRIP, source => source.MapFrom(src => src.NEGO_MONEDA_DESCRIP))
            //.ForMember(destination => destination.MONEDA_CODIGO, source => source.MapFrom(src => src.NEGO_MONEDA_CODIGO));

            /*
            //En caso de que los modelos no sean iguales se debe hacer de la siguiente forma.
            CreateMap<Clase, ClaseDTO>().ReverseMap()
                .ForMember(destination => destination.CLASE_ID, source => source.MapFrom(src => src.CLASE_ID))
                .ForMember(destination => destination.CLASE_DESCRIP, source => source.MapFrom(src => src.CLASE_DESCRIP))
                .ForMember(destination => destination.CLASE_EMPRESA, source => source.MapFrom(src => src.CLASE_EMPRESA))
                .ForMember(destination => destination.CLASE_EDITABLE, source => source.MapFrom(src => src.CLASE_EDITABLE)).ReverseMap();
            */
        }
    }
}
