namespace Crud.Net.Core.DataContractMapper
{
    /// <summary>
    /// Interface for generic dto to entity and entity to dto mapping
    /// </summary>
    public interface ICrudDtoMapper<TEntity, TDto>
    {
        /// <summary>
        /// Maps the given entity to a dto
        /// </summary>
        /// <param name="entity">The entity to map</param>
        /// <returns>The mapped dto</returns>
        TDto MapEntityToDto(TEntity entity);

        /// <summary>
        /// Maps the given entity to the already existing dto
        /// </summary>
        /// <param name="entity">The entity to map</param>
        /// <param name="dto">The already existing dto</param>
        /// <returns>The mapped dto</returns>
        TDto MapEntityToDto(TEntity entity, TDto dto);

        /// <summary>
        /// Maps the given dto to an entity
        /// </summary>
        /// <param name="dto">The dto to map</param>
        /// <returns>The mapped entity</returns>
        TEntity MapDtoToEntity(TDto dto);

        /// <summary>
        /// maps the given dto to the existing entity
        /// </summary>
        /// <param name="dto">The dto to map</param>
        /// <param name="entity">The already existing entity</param>
        /// <returns>The mapped dto</returns>
        TEntity MapDtoToEntity(TDto dto, TEntity entity);
    }
}
