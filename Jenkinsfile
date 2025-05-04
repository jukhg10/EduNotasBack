pipeline {
  agent any

  environment {
    RESOURCE_GROUP  = 'edunotas'
    APP_NAME        = 'edunotas-back'
  }

  stages {
    stage('Build .NET') {
      steps {
        git   branch: 'main', url: 'https://github.com/jukhg10/EduNotasBack.git'
        sh    'dotnet restore'
        sh    'dotnet build --configuration Release --no-restore'
        sh    'dotnet publish --configuration Release --no-restore --output ./publish'
      }
    }

    stage('Deploy to Azure') {
      steps {
        withCredentials([azureServicePrincipal('c221e91e-2364-4f0a-92a1-700af85b1ad0')]) {
          sh '''
            echo "Logging in to Azure..."
            az login --service-principal \
              --username "$AZURE_CLIENT_ID" \
              --password "$AZURE_CLIENT_SECRET" \
              --tenant "$AZURE_TENANT_ID" \
              --output none

            az account set --subscription "$AZURE_SUBSCRIPTION_ID"

            cd publish
            zip -r ../app.zip .
            cd ..

            az webapp deployment source config-zip \
              --resource-group "$RESOURCE_GROUP" \
              --name "$APP_NAME" \
              --src app.zip \
              --output none

            echo " Deployment successful."
          '''
        }
      }
    }
  }

  post {
    success { echo ' Pipeline succeeded ' }
    failure { echo ' Pipeline failed; check logs.' }
  }
}
