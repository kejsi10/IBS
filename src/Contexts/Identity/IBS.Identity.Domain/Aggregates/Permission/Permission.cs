using IBS.BuildingBlocks.Domain;

namespace IBS.Identity.Domain.Aggregates.Permission;

/// <summary>
/// Represents a permission in the system.
/// </summary>
public sealed class Permission : AggregateRoot
{
    /// <summary>
    /// Gets the permission name (e.g., "clients:read").
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the permission description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the module this permission belongs to.
    /// </summary>
    public string Module { get; private set; } = string.Empty;

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private Permission() { }

    /// <summary>
    /// Creates a new permission.
    /// </summary>
    /// <param name="name">The permission name.</param>
    /// <param name="module">The module name.</param>
    /// <param name="description">The permission description (optional).</param>
    /// <returns>A new Permission instance.</returns>
    public static Permission Create(string name, string module, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Permission name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(module))
            throw new ArgumentException("Module is required.", nameof(module));

        return new Permission
        {
            Name = name.Trim().ToLowerInvariant(),
            Module = module.Trim(),
            Description = description?.Trim()
        };
    }

    /// <summary>
    /// Updates the permission description.
    /// </summary>
    /// <param name="description">The new description.</param>
    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
        MarkAsUpdated();
    }
}

/// <summary>
/// Well-known permissions in the system.
/// </summary>
public static class Permissions
{
    /// <summary>
    /// Client module permissions.
    /// </summary>
    public static class Clients
    {
        /// <summary>View clients permission.</summary>
        public const string Read = "clients:read";
        /// <summary>Create clients permission.</summary>
        public const string Create = "clients:create";
        /// <summary>Update clients permission.</summary>
        public const string Update = "clients:update";
        /// <summary>Delete clients permission.</summary>
        public const string Delete = "clients:delete";
    }

    /// <summary>
    /// Policy module permissions.
    /// </summary>
    public static class Policies
    {
        /// <summary>View policies permission.</summary>
        public const string Read = "policies:read";
        /// <summary>Create policies permission.</summary>
        public const string Create = "policies:create";
        /// <summary>Bind policies permission.</summary>
        public const string Bind = "policies:bind";
        /// <summary>Cancel policies permission.</summary>
        public const string Cancel = "policies:cancel";
    }

    /// <summary>
    /// Claims module permissions.
    /// </summary>
    public static class Claims
    {
        /// <summary>View claims permission.</summary>
        public const string Read = "claims:read";
        /// <summary>Create claims permission.</summary>
        public const string Create = "claims:create";
        /// <summary>Update claims permission.</summary>
        public const string Update = "claims:update";
        /// <summary>Close claims permission.</summary>
        public const string Close = "claims:close";
    }

    /// <summary>
    /// Quote module permissions.
    /// </summary>
    public static class Quotes
    {
        /// <summary>View quotes permission.</summary>
        public const string Read = "quotes:read";
        /// <summary>Create quotes permission.</summary>
        public const string Create = "quotes:create";
        /// <summary>Submit quotes permission.</summary>
        public const string Submit = "quotes:submit";
    }

    /// <summary>
    /// Reports module permissions.
    /// </summary>
    public static class Reports
    {
        /// <summary>View reports permission.</summary>
        public const string View = "reports:view";
        /// <summary>Export reports permission.</summary>
        public const string Export = "reports:export";
    }

    /// <summary>
    /// Administration permissions.
    /// </summary>
    public static class Admin
    {
        /// <summary>Manage users permission.</summary>
        public const string Users = "admin:users";
        /// <summary>Manage settings permission.</summary>
        public const string Settings = "admin:settings";
        /// <summary>Manage carriers permission.</summary>
        public const string Carriers = "admin:carriers";
    }
}
