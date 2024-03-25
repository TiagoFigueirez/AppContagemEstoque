using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppContagemEstoque.Model
{
    public class ProdutoModel
    {
        public int Contador { get; set; }
        public string CodigoProduto { get; set; }
        public string Lote { get; set; }
        public string DataValidade { get; set; }
        public int Quantidade { get; set; }
        public int Armazem { get; set; }
    }
}
