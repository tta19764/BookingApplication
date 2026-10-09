using System.Data;
using AutoMapper;
using BookingApp.Bll.Common.Users;
using BookingApp.Bll.Common.Users.Models;
using BookingApp.Dal.SqlServerRepositories.Infrastructure;
using BookingApp.Dal.SqlServerRepositories.Entities;
using BookingApp.Dal.SqlServerRepositories.Mappings;

namespace BookingApp.Dal.SqlServerRepositories.Repositories;

/// <summary>Persists user operations through SQL Server stored procedures.</summary>
/// <param name="connections">Creates a separate pooled connection for each operation.</param>
/// <param name="mapper">Converts DAL entities to and from business models.</param>
public sealed class UserRepository(SqlConnectionFactory connections, IMapper mapper) : IUserRepository
{
    /// <summary>Reads a user by its identifier.</summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">Cancels connection opening and database execution.</param>
    /// <returns>The user, or null when it does not exist.</returns>
    /// <remarks>Loads user roles and their permissions from three result sets before mapping the assembled entity.</remarks>
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await SqlProcedure.ExecuteAsync<User?>(connections, "[TymchenkoOV].[BookingApp.user_get]",
            "UserRepository.GetByIdAsync", async command =>
        {
            command.Parameter("@Id", SqlDbType.UniqueIdentifier, id);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken)) return null;
            var user = RowMapper.User(reader);
            // The procedure returns user, role and role-permission rows in separate result sets.
            await reader.NextResultAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken)) user.Roles.Add(RowMapper.Role(reader));
            await reader.NextResultAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                user.Roles.Single(role => role.Id == reader.GetInt32(reader.GetOrdinal("role_id")))
                    .Permissions.Add(RowMapper.Permission(reader));
            return mapper.Map<User>(user);
        }, cancellationToken);
    }

    /// <summary>Atomically creates a user and their role links.</summary>
    /// <param name="user">The user and existing role identifiers to assign.</param>
    /// <param name="cancellationToken">Cancels connection opening and database execution.</param>
    /// <returns>A task that completes after the user and role links are persisted.</returns>
    /// <remarks>Duplicate role identifiers are collapsed. An invalid role causes the entire database write to roll back.</remarks>
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        var entity = mapper.Map<UserEntity>(user);
        await SqlProcedure.ExecuteAsync(connections, "[TymchenkoOV].[BookingApp.user_create]",
            "UserRepository.AddAsync", async command =>
        {
            command.Parameter("@Id", SqlDbType.UniqueIdentifier, entity.Id);
            command.Parameter("@FirstName", SqlDbType.NVarChar, entity.FirstName, 100);
            command.Parameter("@LastName", SqlDbType.NVarChar, entity.LastName, 100);
            command.Parameter("@Email", SqlDbType.NVarChar, entity.Email, 320);
            using var roleIds = new DataTable();
            roleIds.Columns.Add("Id", typeof(int));
            foreach (var roleId in entity.Roles.Select(role => role.Id).Distinct())
                roleIds.Rows.Add(roleId);
            // A typed empty table creates a user without role links.
            var roles = command.Parameter("@RoleIds", SqlDbType.Structured, roleIds);
            roles.TypeName = "[TymchenkoOV].[BookingApp.RoleIds]";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }, cancellationToken);
    }
}
