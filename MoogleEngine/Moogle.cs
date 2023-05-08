namespace MoogleEngine;


public static class Moogle
{
    public static SearchResult Query(string query, Dictionary<string, float> Universo, Dictionary<string, Dictionary<string, float>> MatrizDocumento) 
    {
        Query Consulta = new Query(query ,Universo);

        CosineSimilarity SimilitudCoseno = new CosineSimilarity(Consulta, Universo, MatrizDocumento);

        Snippet Snippet = new Snippet(Consulta, SimilitudCoseno.DocumentosPuntuados);

              
        SearchItem[] items = new SearchItem[SimilitudCoseno.DocumentosPuntuados.Count];
        

        for (int i = 0; i < SimilitudCoseno.DocumentosPuntuados.Count; i++)
        {
            string Titulo = SimilitudCoseno.DocumentosPuntuados.Keys.ElementAt(i);
            string Texto = Snippet.DocumentosSnippet[Titulo];
            float Score = SimilitudCoseno.DocumentosPuntuados[Titulo];

            items[i] = new SearchItem(Titulo.Split(@"\").Last().Replace(".txt",""), Texto, Score);
        }
        
        if(Consulta.Sugerencia != query.ToLower())
            return new SearchResult(items, Consulta.Sugerencia);
        else 
            return new SearchResult(items);
    }
}
