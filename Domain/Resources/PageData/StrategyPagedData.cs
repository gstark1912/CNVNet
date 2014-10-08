/// <summary>
/// Se utiliza esta clase para la implementación de los distintos tipos de orden a aplicar en una grilla.
/// La idea original es utilizar la clase PagedDataParameters para reconocer que tipo de estrategia aplicar.
/// Luego cada estrategia hereda el método "ApplyStrategy", con el cual se confecciona la query necesaria.
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Domain
{
    public abstract class StrategyPagedData
    {
        public PagedDataParameters Parameters { get; set; }

        public virtual IQueryable<T> ApplyStrategy<T>(IQueryable<T> source)
        {
            return source;
        }

        /// <summary>
        /// Devuelve la instancia de la estrategia a partir de los parámetros enviados.
        /// En este caso hacemos depender la estrategia del orden impuesto por la Grilla.
        /// </summary>                
        public static StrategyPagedData GetStrategy(PagedDataParameters parameters)
        {
            switch (parameters.OrderDirection)
            {
                case Define.OrderBy.Ascendant:
                    return new FullPagedDataAsc(parameters);

                case Define.OrderBy.Descendant:
                    return new FullPagedDataDesc(parameters);

                default:
                    return null;
            }
        }

    }
}
