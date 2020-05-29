﻿using Dapper;
using System;
using System.Linq;
using System.Threading.Tasks;
using Biblia.Domain.Entidades;
using System.Collections.Generic;
using Biblia.Repositorio.Excecoes;
using Microsoft.Extensions.Configuration;

namespace Biblia.Repositorio
{
    public class BibliaRepository : RepoBase, IBibliaRepository
    {
        public BibliaRepository(IConfiguration config) : base(config)
        {
        }

        public async Task<int> ObterQuantidadeCapitulosDoLivroAsync(int livroId)
        {
            using (Conexao)
            {
                try
                {
                    const string query = @"
                                SELECT 
                                    MAX(capitulo) 
                                FROM 
                                    Versiculos WHERE livroId = @cid";

                    return await Conexao.QueryFirstOrDefaultAsync<int>(query, new { cid = livroId });
                }
                catch (Exception ex)
                {
                    throw new ObterQuantidadeCapitulosDoLivroException(ex.Message);
                }
            }
        }

        public async Task<int> ObterQuantidadeVersiculosNoCapituloDoLivroAsync(int idLivro, int idCapitulo)
        {
            using (Conexao)
            {
                try
                {
                    const string query = @"
                                SELECT 
                                    MAX(numero) 
                                FROM 
                                    Versiculos 
                                WHERE 
                                    livroId = @lid AND capitulo = @cid";

                    return await Conexao.QueryFirstOrDefaultAsync<int>(query, new { lid = idLivro, cid = idCapitulo });
                }
                catch (Exception ex)
                {
                    throw new ObterQuantidadeVersiculosNoCapituloDoLivroException(ex.Message);
                }
            }
        }

        public async Task<IEnumerable<Livro>> ListarLivrosAsync(int? testamentoId)
        {
            using (Conexao)
            {
                try
                {
                    var query = $@"
                                    SELECT 
                                        id, testamentoId, posicao, nome 
                                    FROM 
                                        Livros
                                ";
                    if (testamentoId.HasValue)
                    {
                        query += $@"WHERE 
                                        testamentoId = {testamentoId}
                                    ";
                    }

                    return await Conexao.QueryAsync<Livro>(query);

                }
                catch (Exception ex)
                {
                    throw new ListarLivrosException(ex.Message);
                }
            }
        }

        public async Task<IEnumerable<dynamic>> ListarResumosLivrosAsync(int versaoId, int? testamentoId, int? livroId)
        {
            using (Conexao)
            {
                try
                {
                    var query = $@"
                                        SELECT 
                                            t.Id AS TestamentoId, 
                                            t.nome AS Testamento, 
                                            l.Id AS LivroId, 
                                            l.nome AS Livro, 
                                            l.posicao AS Posicao, 
                                            COUNT(DISTINCT(v.capitulo)) AS Capitulos, 
                                            COUNT(v.Id) AS Versiculos
                                        FROM 
                                	        Versiculos v
                                        INNER JOIN 
                                	        Livros l ON v.livroId = l.Id
                                        INNER JOIN 
                                	        Testamentos t ON l.testamentoId = t.Id
                                        WHERE 
                                	        v.versaoId = @vid 
                                    ";

                    if (testamentoId.HasValue)
                    {
                        query += $@"        AND t.Id = {testamentoId} 
                                    ";
                    }

                    if (livroId.HasValue)
                    {
                        query += $@"        AND l.Id = {livroId} 
                                    ";
                    }

                    query += $@"        GROUP BY  
                                            t.Id, 
                                            t.nome, 
                                	        l.Id, 
                                            l.nome, 
                                            l.posicao
                                ";

                    return await Conexao.QueryAsync(query, new { vid = versaoId });
                }
                catch (Exception ex)
                {
                    throw new ListarResumosLivrosException(ex.Message);
                }
            }
        }

        public async Task<IEnumerable<Versao>> ListarVersoesAsync()
        {
            using (Conexao)
            {
                try
                {
                    const string query = @"
                                SELECT 
                                    id,
                                    nome
                                FROM 
                                    Versoes
                                ORDER BY
                                    id
                            ";

                    return await Conexao.QueryAsync<Versao>(query);
                }
                catch (Exception ex)
                {
                    throw new ListarVersoesException(ex.Message);
                }
            }
        }

        public async Task<Versiculo> ObterVersiculoAsync(int versaoId, int livroId, int capitulo, int numero)
        {
            using (Conexao)
            {
                try
                {
                    const string query = @"
                                SELECT 
                                    id, versaoId, capitulo, numero, texto 
                                FROM 
                                    Versiculos 
                                WHERE 
                                    versaoId = @vid 
                                    AND livroId = @lid 
                                    AND capitulo = @cid 
                                    AND numero = @nid;
                                
                                SELECT 
                                    id, nome, testamentoId, posicao 
                                FROM 
                                    Livros 
                                WHERE 
                                    id = @lid;
                            ";

                    using (var result = await Conexao.QueryMultipleAsync(query, new { vid = versaoId, lid = livroId, cid = capitulo, nid = numero }))
                    {
                        var versiculo = result.Read<Versiculo>().First();
                        versiculo.Livro = result.Read<Livro>().Single();

                        return versiculo;
                    }

                }
                catch (Exception ex)
                {
                    throw new ObterVersiculoException(ex.Message);
                }
            }
        }

        public async Task<dynamic> ObterQuantidadeVersiculosNoCapituloAsync(int versaoId, int livroId, int capitulo)
        {
            using (Conexao)
            {
                try
                {
                    const string query = @"
                                                SELECT     
                                                    COUNT(v.Id) AS Versiculos, 
                                                    v.versaoId AS Versao, 
                                                    v.livroId AS Livro, 
                                                    v.capitulo AS Capitulo
                                                FROM 
                                                    Versiculos v
                                                WHERE 
                                                    v.versaoId = @vid
                                                    AND v.livroId = @lid
                                                    AND v.capitulo = @c
                                                GROUP BY
                                                    v.versaoId, v.livroId, v.capitulo
                                            ";

                    return await Conexao.QueryFirstOrDefaultAsync<dynamic>(query, new { vid = versaoId, lid = livroId, c = capitulo });

                }
                catch (Exception ex)
                {
                    throw new ObterQuantidadeVersiculosNoCapituloException(ex.Message);
                }
            }
        }

        public async Task<int> ObterQuantidadeCaixaPromessasAsync()
        {
            using (Conexao)
            {
                try
                {
                    const string query = @"
                                                SELECT     
                                                    MAX(id)
                                                FROM 
                                                    CaixaPromessas c
                                            ";

                    return await Conexao.QueryFirstOrDefaultAsync<int>(query);

                }
                catch (Exception ex)
                {
                    throw new ObterQuantidadeCaixaPromessasException(ex.Message);
                }
            }
        }

        public async Task<IEnumerable<CaixaPromessas>> ObterVersiculosDaCaixaPromessasAsync(int caixaPromessaId)
        {
            using (Conexao)
            {
                try
                {
                    const string query = @"
                                                SELECT
                                                    id, 
                                                    livroId, 
                                                    capituloId, 
                                                    numeroVersiculo
                                                FROM 
                                                    CaixaPromessas v
                                                WHERE 
                                                    v.id = @caixaPromessaId
                                          ";

                    return await Conexao.QueryAsync<CaixaPromessas>(query, new { caixaPromessaId });

                }
                catch (Exception ex)
                {
                    throw new ObterVersiculosDaCaixaPromessasException(ex.Message);
                }
            }
        }

        public async Task<IEnumerable<Versiculo>> ObterVersiculosAsync(int versao, int livro, int capitulo, IEnumerable<int> numeros)
        {
            using (Conexao)
            {
                try
                {
                    const string query = @"
                                SELECT 
                                    id, versaoId, capitulo, numero, texto 
                                FROM 
                                    Versiculos 
                                WHERE 
                                    versaoId = @vid 
                                    AND livroId = @lid 
                                    AND capitulo = @cid 
                                    AND numero IN @nid;
                                
                                SELECT 
                                    id, nome, testamentoId, posicao 
                                FROM 
                                    Livros 
                                WHERE 
                                    id = @lid;
                                          ";

                    return await Conexao.QueryAsync<Versiculo>(query, new { vid = versao, lid = livro, cid = capitulo, nid = numeros });

                }
                catch (Exception ex)
                {
                    throw new ObterVersiculosException(ex.Message);
                }
            }
        }

        public async Task<Livro> ObterLivroAsync(int livroId)
        {
            using (Conexao)
            {
                try
                {
                    const string query = @"
                                                SELECT 
	                                                id, 
                                                    testamentoId,
                                                    posicao,
                                                    nome
                                                FROM 
	                                                Livros
                                                WHERE 
	                                                id =  @lid;
                                          ";

                    return await Conexao.QueryFirstOrDefaultAsync<Livro>(query, new { lid = livroId });

                }
                catch (Exception ex)
                {
                    throw new ObterLivroException(ex.Message);
                }

            }
        }
    }
}
