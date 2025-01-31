﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DeUrgenta.Admin.Api.Models;
using DeUrgenta.Admin.Api.Queries;
using DeUrgenta.Common.Extensions;
using DeUrgenta.Common.Models.Pagination;
using DeUrgenta.Common.Validation;
using DeUrgenta.Domain;
using MediatR;

namespace DeUrgenta.Admin.Api.QueryHandlers
{
    public class GetBlogPostsHandler : IRequestHandler<GetBlogPosts, Result<PagedResult<BlogPostModel>>>
    {
        private readonly IValidateRequest<GetBlogPosts> _validator;
        private readonly DeUrgentaContext _context;

        public GetBlogPostsHandler(IValidateRequest<GetBlogPosts> validator, DeUrgentaContext context)
        {
            _validator = validator;
            _context = context;
        }

        public async Task<Result<PagedResult<BlogPostModel>>> Handle(GetBlogPosts request,
            CancellationToken cancellationToken)
        {
            var isValid = await _validator.IsValidAsync(request);
            if (!isValid)
            {
                return Result.Failure<PagedResult<BlogPostModel>>("Validation failed");
            }

            var pagedBlogPosts = await _context.Blogs
                    .Select(x => new BlogPostModel
                    {
                        Id = x.Id,
                        Author = x.Author,
                        Title = x.Title,
                        ContentBody = x.ContentBody,
                        PublishedOn = x.PublishedOn
                    })
                    .OrderBy(x => x.PublishedOn)
                    .GetPaged(request.Pagination.PageNumber, request.Pagination.PageSize)
                ;
            return pagedBlogPosts;
        }
    }
}