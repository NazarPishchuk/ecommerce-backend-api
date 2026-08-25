using AutoMapper;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Results;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services;

public class ProductService(IUnitOfWork unitOfWork,
                            ICategoryRepository categoryRepository,
                            IProductRepository productRepository,
                            ISellerProfileRepository sellerProfileRepository,
                            IMapper mapper) : IProductService
{
    public async Task<Result> ActivateProductAsync(
        int id,
        string userId,
        CancellationToken cancellationToken)
    {
        var sellerProfile = await GetSellerProfileAsync(
            userId, 
            cancellationToken);

        var product = await productRepository.GetMineByIdForUpdateAsync(
            id, 
            sellerProfile.Id, 
            cancellationToken);

        if (product is null)
        {
            return Result.Failure(new Error(
                "Product.NotFound",
                ErrorType.NotFound,
                "Product with this id was not found."));
        }

        if (product.IsBlocked)
        {
            return Result.Failure(new Error(
                "Product.Blocked",
                ErrorType.Forbidden,
                "Blocked product cannot be activated."));
        }

        product.IsActive = true;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> BlockProductAsync(
        int id, 
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdForUpdateAsync(
            id, 
            cancellationToken);

        if(product is null)
        {
            return Result.Failure(new Error(
                "Product.NotFound",
                ErrorType.NotFound,
                "Product with this id was not found."));
        }

        product.IsBlocked = true;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<GetSellerProductDto>> CreateProductAsync(
        string userId,
        CreateProductDto dto, 
        CancellationToken cancellationToken)
    {
        var sellerProfile = await GetSellerProfileAsync(userId, cancellationToken);

        var category = await categoryRepository.GetByIdAsync(
            dto.CategoryId,
            cancellationToken);

        if (category is null)
        {
            return Result<GetSellerProductDto>.Failure(
                new Error(
                    "Category.NotFound",
                    ErrorType.NotFound,
                    "Category was not found."));
        }

        var product = mapper.Map<Product>(dto);

        product.SellerProfileId = sellerProfile.Id;
        product.IsActive = true;
        product.IsBlocked = false;

        product.Category = category;

        productRepository.Add(product);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var productDto = mapper.Map<GetSellerProductDto>(product);

        return Result<GetSellerProductDto>.Success(productDto);
    }

    public async Task<Result> DeactivateProductAsync(
        int id, 
        string userId, 
        CancellationToken cancellationToken)
    {
        var sellerProfile = await GetSellerProfileAsync(
            userId,
            cancellationToken);

        var product = await productRepository.GetMineByIdForUpdateAsync(
            id,
            sellerProfile.Id,
            cancellationToken);

        if (product is null)
        {
            return Result.Failure(new Error(
                "Product.NotFound",
                ErrorType.NotFound,
                "Product with this id was not found."));
        }

        product.IsActive = false;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
    

    public async Task<Result<IReadOnlyList<GetProductListDto>>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);

        var productsDto = mapper.Map<IReadOnlyList<GetProductListDto>>(products);

        return Result<IReadOnlyList<GetProductListDto>>.Success(productsDto);
    }

    public async Task<Result<GetProductDto>> GetByIdAsync(
        int id, 
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken);

        if(product is null)
        {
            return Result<GetProductDto>.Failure(
                new Error(
                    "Product.NotFound",
                    ErrorType.NotFound,
                    "Product with this id was not found."));
        }

        var productDto = mapper.Map<GetProductDto>(product);

        return Result<GetProductDto>.Success(productDto);
    }

    public async Task<Result<IReadOnlyList<GetSellerProductDto>>> GetAllMineAsync(
        string userId, 
        CancellationToken cancellationToken)
    {
        var sellerProfile = await GetSellerProfileAsync(userId, cancellationToken);

        var products = await productRepository.GetAllMineAsync(
            sellerProfile.Id, 
            cancellationToken);

        var producsDto = mapper.Map<IReadOnlyList<GetSellerProductDto>>(products);

        return Result<IReadOnlyList<GetSellerProductDto>>.Success(producsDto);
    }

    public async Task<Result<GetSellerProductDto>> GetMineByIdAsync(
        int id,
        string userId, 
        CancellationToken cancellationToken)
    {
        var sellerProfile = await GetSellerProfileAsync(userId, cancellationToken);

        var products = await productRepository.GetMineByIdAsync(
            id,
            sellerProfile.Id,
            cancellationToken);

        if (products is null)
        {
            return Result<GetSellerProductDto>.Failure(
                new Error(
                    "Product.NotFound",
                    ErrorType.NotFound,
                    "Product with this id was not found."));
        }

        var productsDto = mapper.Map<GetSellerProductDto>(products);

        return Result<GetSellerProductDto>.Success(productsDto);
    }

    public async Task<Result> UnblockProductAsync(
        int id, 
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdForUpdateAsync(
            id,
            cancellationToken);

        if (product is null)
        {
            return Result.Failure(new Error(
                "Product.NotFound",
                ErrorType.NotFound,
                "Product with this id was not found."));
        }

        product.IsBlocked = false;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpdateProductAsync(
        int id, 
        string userId,
        UpdateProductDto dto, 
        CancellationToken cancellationToken)
    {
        var sellerProfile = await GetSellerProfileAsync(userId, cancellationToken);

        var product = await productRepository.GetMineByIdForUpdateAsync(
            id, 
            sellerProfile.Id, 
            cancellationToken);

        if(product is null)
        {
            return Result.Failure(new Error(
                "Product.NotFound",
                ErrorType.NotFound,
                "Product with this id was not found."));
        }

        var category = await categoryRepository.GetByIdAsync(
            dto.CategoryId,
            cancellationToken);

        if (category is null)
        {
            return Result.Failure(new Error(
                "Category.NotFound",
                ErrorType.NotFound,
                "Category with this id was not found."));
        }

        mapper.Map(dto, product);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<SellerProfile> GetSellerProfileAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var sellerProfile = await sellerProfileRepository.GetByUserIdAsNoTrackingAsync(
            userId,
            cancellationToken);

        if (sellerProfile is null)
        {
            throw new InvalidOperationException(
                "User with Seller role does not have a seller profile.");
        }

        return sellerProfile; 
    }
}
