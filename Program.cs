using Services;
using Models;

do
{
    Console.Clear();
    Console.WriteLine("--- MENU ---");
    Console.WriteLine("1- Cadastrar Produto");
    Console.WriteLine("2- Listar Produtos");
    Console.WriteLine("3- Buscar Produto");
    Console.WriteLine("4- Alterar Nome");
    Console.WriteLine("5- Atualizar Preço");
    Console.WriteLine("6- Estocar");
    Console.WriteLine("7- Excluir Produto");
    Console.WriteLine("8- Criar Pedido");
    Console.WriteLine("9- Listar Pedidos");
    Console.WriteLine("10- Cancelar Pedidos");
    Console.WriteLine("0- SAIR");
 
    var opcao = Console.ReadLine();
 
    switch (opcao)
    {
        case "1":
            Console.WriteLine("Digite o nome do produto:");
            var nome = Console.ReadLine();
            if (string.IsNullOrEmpty(nome))
            {
                Console.WriteLine("O nome não pode ser vazio");
                break;
            }
 
            Console.WriteLine("Digite o preço do produto:");
            var preco = decimal.Parse(Console.ReadLine()!);
 
            Console.WriteLine("Digite a quantidade em estoque:");
            var estoque = int.Parse(Console.ReadLine()!);
 
            var novoProduto = await ProdutoService.CreateProduto(nome!, preco, estoque);
            Console.WriteLine($"Produto de ID: {novoProduto.Id} | Produto: {novoProduto.Nome} adicionado com sucesso");
            Console.ReadLine();
            break;
           
        case "2":
            var produtos = await ProdutoService.ListarProdutos();
            foreach (var p in produtos)
            {
                Console.WriteLine($"ID: {p.Id}, Nome: {p.Nome} | Preço: {p.Preco}R$ | Estoque: {p.Estoque}");
            }
            Console.ReadLine();
            break;
 
        case "3":
            Console.WriteLine("Digite o nome do produto para buscar:");
            var nome1 = Console.ReadLine();
            var resultado = await ProdutoService.BuscarPorNome(nome1);
            foreach (var r in resultado)
            {
                Console.WriteLine($"ID: {r.Id} | Nome: {r.Nome} | Preço: {r.Preco}R$ | Estoque: {r.Estoque}");
            }
            Console.ReadLine();
            break;

        case "4":
            Console.WriteLine("Digite o ID:");
            var id = int.Parse(Console.ReadLine()!);
            Console.WriteLine("Digite o novo nome do produto:");
            var nome2 = Console.ReadLine();
            await ProdutoService.AlterarNome(id, nome2);
            Console.ReadLine();
            break;
        
        case "5":
            Console.WriteLine("Digite o ID:");
            var id1 = int.Parse(Console.ReadLine()!);
            Console.WriteLine("Digite o novo preço do produto:");
            var preco1 = decimal.Parse(Console.ReadLine()!);
            await ProdutoService.AtualizarPreco(id1, preco1);
            Console.ReadLine();
            break;
        
        case "6":
            Console.WriteLine("Digite o ID:");
            var id2 = int.Parse(Console.ReadLine()!);
            Console.WriteLine("Digite o quando será adicionado ao estoque do produto:");
            var estoque1 = int.Parse(Console.ReadLine()!);
            await ProdutoService.Estocar(id2, estoque1);
            Console.ReadLine();
            break;

        case "7":
            Console.WriteLine("Digite o ID:");
            var id3 = int.Parse(Console.ReadLine()!);
            await ProdutoService.DeletarProduto(id3);
            Console.ReadLine();
            break;
        
        case "8":
                Console.WriteLine("Digite o ID do usuário:");
                int usuarioId = int.Parse(Console.ReadLine()!);

                var itensPedidos = new List<ItemPedido>();
                string continuar = "s";

                while (continuar.ToLower() == "s")
                {
                    Console.WriteLine("Digite o ID do produto:");
                    int produtoId = int.Parse(Console.ReadLine()!);

                    Console.WriteLine("Digite a quantidade:");
                    int qtd = int.Parse(Console.ReadLine()!);

                    itensPedidos.Add(new ItemPedido
                    {
                        ProdutoId = produtoId,
                        Quantidade = qtd
                    });

                    Console.Write("Adicionar outro produto? (s/n): ");
                    continuar = Console.ReadLine() ?? "n";
                }

                var novoPedido = await PedidoService.CreatePedido(usuarioId, itensPedidos);
                if (novoPedido != null)
                {
                    Console.WriteLine($"Pedido de ID:{novoPedido.Id} foi criado | Total: {novoPedido.ValorTotal}R$");
                }
                Console.ReadLine();
                break;

            case "9":
                var pedidos = await PedidoService.ListarPedidos();
                foreach (var p in pedidos)
                {
                    Console.WriteLine($"\nPedido de ID{p.Id} | Status: {p.Status} | Cliente: {p.Usuario.Nome} | Total: {p.ValorTotal}R$");
                    foreach (var item in p.Itens)
                    {
                        Console.WriteLine($"   {item.Quantidade} x {item.Produto.Nome} ({item.PrecoUnitario}R$)");
                    }
                }
                Console.ReadLine();
                break;

            case "10":
                Console.WriteLine("Digite o ID do pedido para cancelar:");
                int pedidoId = int.Parse(Console.ReadLine()!);
                await PedidoService.CancelarPedido(pedidoId);
                Console.ReadLine();
                break;

            case "0":
                return;

            default:
                Console.WriteLine("Opção inválida!");
                break;
    }
} while (true);