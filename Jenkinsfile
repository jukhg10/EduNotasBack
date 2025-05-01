pipeline {
    agent any

    environment {
        RESOURCE_GROUP = 'edunotas'
        APP_NAME = 'edunotas-back'
        TENANT_ID = 'cba97d17-fa68-4044-96f8-3ac26469a389'
        SUBSCRIPTION_ID = 'e8381082-e03f-4d7c-8155-4bc17503a57'
    }

    stages {
        stage('Clonar Repositorio') {
            steps {
                git branch: 'main', url: 'https://github.com/jukhg10/EduNotasBack.git'
            }
        }

        stage('Restaurar Dependencias') {
            steps {
                sh 'dotnet restore'
            }
        }

        stage('Compilar') {
            steps {
                sh 'dotnet build --configuration Release --no-restore'
            }
        }

        stage('Publicar') {
            steps {
                sh 'dotnet publish --configuration Release --no-restore --output ./publish'
            }
        }

        stage('Desplegar a Azure') {
            steps {
                withCredentials([azureServicePrincipal('b67c5787-3a3b-434a-826e-63cd5730a744')]) {
                    sh '''
                        echo "Iniciando sesión en Azure..."
                        az login --service-principal -u $AZURE_CREDENTIALS_USR -p $AZURE_CREDENTIALS_PSW --tenant $TENANT_ID > /dev/null

                        echo "Seleccionando suscripción..."
                        az account set --subscription $SUBSCRIPTION_ID

                        echo "Comprimiendo archivos para despliegue..."
                        cd publish
                        zip -r ../app.zip .
                        cd ..

                        echo "Desplegando a Azure Web App..."
                        az webapp deployment source config-zip \
                          --resource-group $RESOURCE_GROUP \
                          --name $APP_NAME \
                          --src app.zip

                        echo "Despliegue completado exitosamente."
                    '''
                }
            }
        }
    }

    post {
        failure {
            echo " Error en el pipeline. Revisa los logs para más detalles."
        }
        success {
            echo " Pipeline ejecutado correctamente. Cambios desplegados a Azure."
        }
    }
}
