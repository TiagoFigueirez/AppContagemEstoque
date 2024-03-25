using AppContagemEstoque.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using OfficeOpenXml;
using System.Diagnostics;
using ComponentFactory.Krypton.Toolkit;
using System.Linq;


namespace AppContagemEstoque
{
    public partial class frmPuxarProduto : KryptonForm
    {
        public string codigo;
        public bool lendoCodigoDeBarras = false;
        public int contador = 1;
        public string DiretorioSelecionado;
        public List<ProdutoModel> produtos = new List<ProdutoModel>();

        public frmPuxarProduto()
        {
            InitializeComponent();

        }

        private void frmPuxarProduto_Load(object sender, EventArgs e)
        {
            txtProduto.Focus();
        }

        private async void txtProduto_KeyPress(object sender, KeyPressEventArgs e)
        {
            codigo += e.KeyChar;

            if (!lendoCodigoDeBarras)
            {
                lendoCodigoDeBarras = true;

                await LerCodigoBarras();
            }
        }

        private void btnGerarPlanilha_Click(object sender, EventArgs e)
        {
            string fileName = "listaContagem.xlsx";

            if (DiretorioSelecionado == null || DiretorioSelecionado == "")
            {
                MessageBox.Show("Informe um Local Para Salvar o Arquivo");
                return;
            }

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            using (var package = new ExcelPackage())
            {
                var woorksheet = package.Workbook.Worksheets.Add("Produtos");

                woorksheet.Cells[1, 1].Value = "Contagem";
                woorksheet.Cells[1, 2].Value = "Codigo do Produto";
                woorksheet.Cells[1, 3].Value = "Lote";
                woorksheet.Cells[1, 4].Value = "Data de validade";

                int row = 2;
                foreach (var produtos in this.produtos)
                {
                    woorksheet.Cells[row, 1].Value = produtos.Contador;
                    woorksheet.Cells[row, 2].Value = produtos.CodigoProduto;
                    woorksheet.Cells[row, 3].Value = produtos.Lote;
                    woorksheet.Cells[row, 4].Value = produtos.DataValidade;

                    row++;
                }

                string filePath = Path.Combine(DiretorioSelecionado, fileName);
                package.SaveAs(new System.IO.FileInfo(filePath));
                Process.Start(filePath);
            }
        }

        private void btnDiretorio_Click(object sender, EventArgs e)
        {
            DiretorioSelecionado = null;

            FolderBrowserDialog _folderBrowserDialog = new FolderBrowserDialog();

            _folderBrowserDialog.Description = "Selecione um diretório";


            //mostra a pasta para selecionar o destino do arquivo 
            DialogResult result = _folderBrowserDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                DiretorioSelecionado = _folderBrowserDialog.SelectedPath;
                txtProduto.Focus();
            }
        }

        private void txtLimparDados_Click(object sender, EventArgs e)
        {
            DialogResult ConfirmarExclusao = MessageBox.Show("A contagem será ZERADA! deseja continua ?", "Atenção!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (ConfirmarExclusao == DialogResult.Yes)
            {
                produtos.Clear();
                LimparGridView();
                txtProduto.Focus();

                contador = 1;
            }
            else if (ConfirmarExclusao == DialogResult.No)
            {
                txtProduto.Focus();
            }
        }
        private void btnExluir_Click(object sender, EventArgs e)
        {

            if (dgvProduto.SelectedCells.Count > 0)
            {
                int contador = Convert.ToInt32(dgvProduto.SelectedCells[0].Value);

                ProdutoModel produtoExcluido = produtos.Find(produto => produto.Contador == contador);

                DialogResult confirmarExclusão = MessageBox.Show("Deseja realmene excluir o produto da contagem ?", "atenção!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirmarExclusão == DialogResult.Yes)
                {
                    produtos.Remove(produtoExcluido);
                    txtProduto.Focus();

                    RenumerarContador();

                    LimparGridView();
                    CarregarGridView();
                }
                else
                {
                    txtProduto.Focus();
                }
            }
        }

        private async Task LerCodigoBarras()
        {
            int tempoLimite = 1000;

            await Task.Delay(tempoLimite);
            AdicionarProduto(codigo);

            codigo = "";
            txtProduto.Clear();
            lendoCodigoDeBarras = false;
        }

        public void AdicionarProduto(string produto)
        {
            string[] produtoDividido = produto.Split(' ');

            ProdutoModel _produtoModel = new ProdutoModel();

            try
            {
                _produtoModel.CodigoProduto = produtoDividido[0];
                _produtoModel.Lote = produtoDividido[1];
                _produtoModel.DataValidade = produtoDividido[2];


                ProdutoModel encontrarProduto = produtos.FirstOrDefault(p => p.Lote == _produtoModel.Lote);

                if (encontrarProduto != null)
                {
                    encontrarProduto.Quantidade++;
                    LimparGridView();
                    CarregarGridView();
                }
                else
                {
                    _produtoModel.Contador = contador;
                    _produtoModel.Quantidade = 1;

                    produtos.Add(_produtoModel);

                    LimparGridView();
                    CarregarGridView();
                    ++contador;
                }

            }
            catch
            {
                txtProduto.Text = null;
                MessageBox.Show("Falha ao ler o codigo, tente novamente", "atenção");
                return;
            }
        }        

        private void LimparGridView()
        {
            dgvProduto.Rows.Clear();
        }

        public void CarregarGridView()
        {
            int ultimaLinha = dgvProduto.Rows.Count - 1;

            foreach (var produtosGrid in produtos)
            {
                dgvProduto.Rows.Add(produtosGrid.Contador, produtosGrid.CodigoProduto, produtosGrid.Lote, produtosGrid.DataValidade, produtosGrid.Quantidade);
            }

            if (ultimaLinha > 0)
            {
                dgvProduto.FirstDisplayedScrollingRowIndex = ultimaLinha;
                dgvProduto.Rows[ultimaLinha].Selected = true;
                dgvProduto.CurrentCell = dgvProduto.Rows[ultimaLinha].Cells[0];
            }
        }

        private void RenumerarContador()
        {
            int contador = 0;

            foreach (var produto in produtos)
            {
                produto.Contador = ++contador;
            }
            ProdutoModel PegarUltimoContador = produtos[produtos.Count - 1];

            this.contador = PegarUltimoContador.Contador;
            this.contador++;
        }
    }
}

