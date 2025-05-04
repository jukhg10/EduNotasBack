pipeline {
  agent any

  stages {
    stage('Build & Deploy') {
      agent {
        docker { 
          image 'mcr.microsoft.com/dotnet/sdk:8.0' 
          args '-u root'         // para que pueda zipper y usar az cli si lo instalas ahí
        }
      }
      environment {
        AZURE_CREDENTIALS = credentials('b67c5787-3a3b-434a-826e-63cd5730a744')
        RESOURCE_GROUP   = 'edunotas'
        APP_NAME         = 'edunotas-back'
        TENANT_ID        = 'cba97d17-fa68-4044-96f8-3ac26469a389'
        SUBSCRIPTION_ID  = 'e8381082-e03f-4d7c-8155-4bc17503a57'
      }
      stages {
        stage('Restore') {
          steps { sh 'dotnet restore' }
        }
        stage('Build') {
          steps { sh 'dotnet build --configuration Release --no-restore' }
        }
        stage('Publish') {
          steps { sh 'dotnet publish --configuration Release --no-restore --output ./publish' }
        }
        stage('Deploy') {
          steps {
            // si azure cli no está, instala rápido (sin persistir)
            sh '''
              apt-get update && apt-get install -y apt-transport-https ca-certificates curl gnupg lsb-release
              curl -sL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > /usr/share/keyrings/microsoft-archive-keyring.gpg
              echo "deb [signed-by=/usr/share/keyrings/microsoft-archive-keyring.gpg] https://packages.microsoft.com/repos/azure-cli/ $(lsb_release -cs) main" \
                > /etc/apt/sources.list.d/azure-cli.list
              apt-get update && apt-get install -y azure-cli
            '''
            withCredentials([azureServicePrincipal('b67c5787-3a3b-434a-826e-63cd5730a744')]) {
              sh '''
                az login --service-principal -u $AZURE_CREDENTIALS_USR -p $AZURE_CREDENTIALS_PSW --tenant $TENANT_ID
                az account set --subscription $SUBSCRIPTION_ID
                cd publish && zip -r ../app.zip . && cd ..
                az webapp deployment source config-zip --resource-group $RESOURCE_GROUP --name $APP_NAME --src app.zip
              '''
            }
          }
        }
      }
    }
  }
}
