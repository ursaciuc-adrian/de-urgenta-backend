﻿using CSharpFunctionalExtensions;
using DeUrgenta.Certifications.Api.Models;
using DeUrgenta.Certifications.Api.Queries;
using DeUrgenta.Common.Validation;
using DeUrgenta.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeUrgenta.Certifications.Api.Storage;

namespace DeUrgenta.Certifications.Api.QueryHandlers
{
    public class GetCertificationsHandler : IRequestHandler<GetCertifications, Result<IImmutableList<CertificationModel>>>
    {
        private readonly IValidateRequest<GetCertifications> _validator;
        private readonly DeUrgentaContext _context;
        private readonly IBlobStorage _storage;

        public GetCertificationsHandler(IValidateRequest<GetCertifications> validator, DeUrgentaContext context, IBlobStorage storage)
        {
            _validator = validator;
            _context = context;
            _storage = storage;
        }

        public async Task<Result<IImmutableList<CertificationModel>>> Handle(GetCertifications request,
            CancellationToken cancellationToken)
        {
            var isValid = await _validator.IsValidAsync(request);
            if (!isValid)
            {
                return Result.Failure<IImmutableList<CertificationModel>>("Validation failed");
            }

            var certifications = await _context.Certifications
            .Where(x => x.User.Sub == request.UserSub)
            .Select(x => new CertificationModel
            {
                Id = x.Id,
                Name = x.Name,
                IssuingAuthority = x.IssuingAuthority,
                ExpirationDate = x.ExpirationDate,
                PhotoUrl = _storage.GetAttachment(x.Id, request.UserSub)
            })
            .ToListAsync(cancellationToken);

            return certifications.ToImmutableList();
        }
    }
}