using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;
using System.IO;
using FITkms.Models;

namespace FITkms.Lucene
{
    public static class LuceneSearch
    {
        //_luceneDir je puna fizička putanja do lucene_index foldera (u root-u aplikacije).
        public static string _luceneDir = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "lucene_index");
        private static FSDirectory _directoryTemp; //FSDirectory je bazna klasa za impl. direktorija koji pohranjuju index file-ove na file sistemu.
        //_directory je istanca Lucene.Net FSDirectory klase i biti će korištena od strane svih search funkcija za pristup search index-u.
        private static FSDirectory _directory
        {
            get
            {
                if (_directoryTemp == null) _directoryTemp = FSDirectory.Open(new DirectoryInfo(_luceneDir));
                if (IndexWriter.IsLocked(_directoryTemp)) IndexWriter.Unlock(_directoryTemp); //IndexWriter kreira index i upravlja njime.
                var lockFilePath = Path.Combine(_luceneDir, "write.lock");
                if (File.Exists(lockFilePath)) File.Delete(lockFilePath);
                return _directoryTemp;
            }
        }

        private static void _addToLuceneIndex(Articles articles, IndexWriter writer)
        {
            //uklanjanje starog index unosa
            var searchQuery = new TermQuery(new Term("ArticleID", articles.ArticleID.ToString()));
            writer.DeleteDocuments(searchQuery);

            //dodavanje novog index unosa
            var doc = new Document();

            //mapiranje db atributa
            doc.Add(new Field("ArticleID", articles.ArticleID.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED)); //NOT_ANALYZED se koristi za jedinstvene vrijednosti.
            doc.Add(new Field("Title", articles.Title, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("Content", articles.Content, Field.Store.YES, Field.Index.ANALYZED)); //ANALYZED se koristi za text ili string.

            //dodavanje u index
            writer.AddDocument(doc);
        }
        #region funkcije za dodavanje podataka u Lucene search index

        public static void AddUpdateLuceneIndex(IEnumerable<Articles> articles) //Dodavanje liste unosa u search index
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30); //Standardni analizer kreira analizer sa uključenim "stop" riječima.
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                //dodavanje podataka u search index (zamjenjuje prošli unos ako postoji)
                foreach (var article in articles)
                {
                    _addToLuceneIndex(article, writer);
                }

                //obavezno zatvoriti StandardAnalyzer i IndexWriter
                analyzer.Close();
                writer.Dispose();
            }
        }

        public static void AddUpdateLuceneIndex(Articles article) //Dodavanje samo jednog unosa u search index
        {
            AddUpdateLuceneIndex(new List<Articles> { article });
        }
        #endregion

        #region funkcije za brisanje i optimizaciju podataka u Lucene search indexu

        //Ukoliko izbrišemo članak iz baze, moramo i iz Lucene search indexa
        public static void ClearLuceneIndexRecord(int record_id)
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                //brisanje
                var searchQuery = new TermQuery(new Term("ArticleID", record_id.ToString()));
                writer.DeleteDocuments(searchQuery);

                analyzer.Close();
                writer.Dispose();
            }
        }

        public static bool ClearLuceneIndex()
        {
            try
            {
                var analyzer = new StandardAnalyzer(Version.LUCENE_30);
                using (var writer = new IndexWriter(_directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    writer.DeleteAll();

                    analyzer.Close();
                    writer.Dispose();
                }
            }
            catch (Exception)
            {

                return false;
            }
            return true;
        }

        public static void Optimize()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                analyzer.Close();
                writer.Optimize(); //za brži search :)
                writer.Dispose();
            }
        }
        #endregion

        #region funkcije za mapiranje Lucene search indexa sa Artiklima

        private static Articles _mapLuceneDocumentToData(Document doc)
        {
            return new Articles
            {
                ArticleID = Convert.ToInt32(doc.Get("ArticleID")),
                Title = doc.Get("Title"),
                Content = doc.Get("Content")
            };
        }

        private static IEnumerable<Articles> _mapLuceneToDataList(IEnumerable<Document> hits)
        {
            return hits.Select(_mapLuceneDocumentToData).ToList();
        }

        private static IEnumerable<Articles> _mapLuceneToDataList(IEnumerable<ScoreDoc> hits, IndexSearcher searcher)
        {
            return hits.Select(hit => _mapLuceneDocumentToData(searcher.Doc(hit.Doc))).ToList();
        }

        #endregion

        #region search funkcije

        private static Query parseQuery(string searchQuery, QueryParser parser) //parsiranje našeg queryja u Lucene query objekat
        {
            Query query;
            try
            {
                query = parser.Parse(searchQuery.Trim());
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }

            return query;
        }

        //glavna search funkcija
        private static IEnumerable<Articles> _search(string searchQuery, string searchField = "")
        {
            if (string.IsNullOrEmpty(searchQuery.Replace("*", "").Replace("?", "")))
            {
                return new List<Articles>();
            }

            //Lucene pretraživač
            using (var searcher = new IndexSearcher(_directory, false))
            {
                var hits_limit = 1000; //Kada lucene prođe kroz 1000 search rezultata postaje dosta sporiji, tako da ga ovako limitiramo.
                var analyzer = new StandardAnalyzer(Version.LUCENE_30);

                //pretraga na osnovu jednog polja
                if (!string.IsNullOrEmpty(searchField))
                {
                    var parser = new QueryParser(Version.LUCENE_30, searchField, analyzer);
                    var query = parseQuery(searchQuery, parser);
                    var hits = searcher.Search(query, hits_limit).ScoreDocs;
                    var results = _mapLuceneToDataList(hits, searcher);
                    analyzer.Close();
                    searcher.Dispose();
                    return results;
                }
                //pretraga na osnovu više polja
                else
                {
                    var parser = new MultiFieldQueryParser(Version.LUCENE_30, new[] { "ArticleID", "Title", "Content" }, analyzer);
                    var query = parseQuery(searchQuery, parser);
                    var hits = searcher.Search(query, null, hits_limit, Sort.RELEVANCE).ScoreDocs;
                    var results = _mapLuceneToDataList(hits, searcher);
                    analyzer.Close();
                    searcher.Dispose();
                    return results;
                }
            }
        }

        #endregion

        #region public funkcije za poziv search funkcija

        public static IEnumerable<Articles> Search(string input, string fieldName = "")
        {
            if (string.IsNullOrEmpty(input))
            {
                return new List<Articles>();
            }

            var terms = input.Trim().Replace("-", " ").Split(' ').Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim() + "*");
            input = string.Join(" ", terms);

            return _search(input, fieldName);
        }

        public static IEnumerable<Articles> SearchDefault(string input, string fieldName = "")
        {
            return string.IsNullOrEmpty(input) ? new List<Articles>() : _search(input, fieldName);
        }

        //čitav lucene search index
        public static IEnumerable<Articles> GetAllIndexRecords()
        {
            if (!System.IO.Directory.EnumerateFiles(_luceneDir).Any())
            {
                return new List<Articles>();
            }

            var searcher = new IndexSearcher(_directory, false);
            var reader = IndexReader.Open(_directory, false);
            var docs = new List<Document>(); //Dokument je dio indexa i searcha.On je set polja. Svako polje ima svoje ime i text.
            var term = reader.TermDocs();
            while (term.Next())
            {
                docs.Add(searcher.Doc(term.Doc));   
            }
            reader.Dispose();
            searcher.Dispose();
            return _mapLuceneToDataList(docs);
        }


        #endregion



        //Korištenje
        //LuceneSearch.AddUpdateLuceneIndex(SampleDataRepository.GetAll()); 

        //ili

        //var new_record = new SampleData {Id = X, Name = "SomeName", Description = "SomeDescription"};
        //LuceneSearch.AddUpdateLuceneIndex(new_record);  

    }

}