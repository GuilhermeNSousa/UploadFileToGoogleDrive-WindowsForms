﻿using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SendFileToDriveWinForm
{
    public class UploadDrive
    {
        const string sIdPastaDestino = "Id da pasta desejada no Google Drive";
        const string caminhoDoArquivoLocal = "Caminho para a pasta em que o arquivo está localizado";
        const string caminhoDasCredenciais = "Caminho para a pasta em que a credencial em .json está localizada";

        //static void Main(string[] args)
        //{

        //    UserCredential credential = ObtemAutorização();
        //    var service = CriaDivreService(credential);
        //    string nomeDoArquivoNoDrive = "nomeArquivoNoDrive";
        //    var fileMetadata = CriaArquivoNoDrive(nomeDoArquivoNoDrive, sIdPastaDestino);

        //    //ExcluiArquivosNaPasta(service, sIdPastaDestino);

        //    string sLink = UploadArquivoParaDrive(service, fileMetadata, nomeDoArquivoNoDrive);
        //}

        public UserCredential ObtemAutorização()
        {
            try
            {
                var tokenStorage = new FileDataStore(caminhoDasCredenciais);

                UserCredential credential;

                using (var stream = new FileStream(caminhoDasCredenciais, FileMode.Open, FileAccess.Read))
                {
                    // Pede autenticação ou carrega a última autenticação armazenada pro userName
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                            new[] { DriveService.ScopeConstants.DriveFile },
                            "teste",
                            CancellationToken.None,
                            tokenStorage).Result;
                }

                return credential;

            }
            catch (Exception err)
            {
                var teste = err.Message.ToString();
                return null;
            }
        }

        public DriveService CriaDivreService(UserCredential credential)
        {
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Upload to drive",
            });

            return service;
        }

        public Google.Apis.Drive.v3.Data.File CriaArquivoNoDrive(string nomeDoArquivoNoDrive, string idPastaDestino)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = nomeDoArquivoNoDrive,
                Parents = new List<string>() { idPastaDestino }
            };

            return fileMetadata;
        }

        public string UploadArquivoParaDrive(DriveService service, Google.Apis.Drive.v3.Data.File fileMetadata, string nomeDoArquivoNoDrive)
        {

            var sNomeArquivoLocal = caminhoDoArquivoLocal + "SPDL - Lista de Ramais.pdf";

            using (var stream = new FileStream(sNomeArquivoLocal, FileMode.Open))
            {
                FilesResource.CreateMediaUpload request;
                request = service.Files.Create(fileMetadata, stream, "application/pdf");
                request.Fields = "id";
                request.Upload();
                var file = request.ResponseBody;

                Console.WriteLine($"Arquivo '{nomeDoArquivoNoDrive}' adicionado ao Google Drive com o ID: {file.Id}");

                string LinkArquivo = $"https://drive.google.com/file/d/{file.Id}/view?usp=sharing";

                Console.WriteLine($"Link para o arquivo: {LinkArquivo}\n");

                return LinkArquivo;

            }

        }

        public void ExcluiArquivosNaPasta(DriveService service, string idPastaDestino)
        {
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.Q = $"'{idPastaDestino}' in parents";
            listRequest.Fields = "nextPageToken, files(id, name)";

            // Executar a solicitação de listagem de arquivos
            var fileList = listRequest.Execute();

            // Obter a lista de arquivos retornados pela solicitação
            IList<Google.Apis.Drive.v3.Data.File> files = fileList.Files;

            //Arquivos foram encontrados
            if (files.Count > 0)
            {
                Console.WriteLine($"Arquivos a serem excluidos:\n");
                foreach (var file in files)
                {
                    Console.WriteLine($"ID: {file.Id} - Nome: {file.Name}");

                    var fileId = $"{file.Id}";
                    service.Files.Delete(fileId).Execute();
                    Console.WriteLine($"Arquivo deletado: {fileId} \n");
                }
            }
            // Arquivos não foram encontrados
            else
            {
                Console.WriteLine("Nenhum arquivo encontrado no Google Drive.");
            }
        }
    }
}