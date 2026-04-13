using UnityEngine;
using System.Text;

public static class StringBuilderExtensions
{
    public static StringBuilder QuoteSafe( this StringBuilder builder, string value )
    {
        builder.Append( '"' );
        int pendingIndex = 0;
        for ( int i = 0; i < value.Length; i++ )
        {
            var c = value[ i ];

            if ( c == '"' )
            {
                // Add strings in blocks
                int count = i - pendingIndex;
                if ( count > 0 )
                {
                    builder.Append( value, pendingIndex, count );
                }

                // Escape double quotes
                builder.Append( "\\\"" );
                pendingIndex = i + 1;
            }
        }
        // Add remaining on end
        if ( pendingIndex < value.Length )
        {
            builder.Append( value, pendingIndex, value.Length - pendingIndex );
        }
        builder.Append( '"' );
        return builder;
    }
}
