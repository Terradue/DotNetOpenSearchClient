pipeline {
  agent { node { label 'docker' } }
  environment {
      VERSION_TOOL = getVersionFromCsProj('Terradue.OpenSearch.Client/Terradue.OpenSearch.Client.csproj')
      VERSION_TYPE = getTypeOfVersion(env.BRANCH_NAME)
      CONFIGURATION = getConfiguration(env.BRANCH_NAME)
      GITHUB_ORGANIZATION = 'Terradue'
      GITHUB_REPO = 'DotNetOpenSearchClient'
  }
  stages {
    stage('.Net Core') {
      agent { 
          docker { 
              image 'mcr.microsoft.com/dotnet/sdk:5.0-buster-slim'
              args '-v /var/run/docker.sock:/var/run/docker.sock --group-add 2057'
          } 
      }
      environment {
        DOTNET_CLI_HOME = "/tmp/DOTNET_CLI_HOME"
      }
      stages {
        stage("Build & Test") {
          steps {
            echo "Build .NET application"
            sh "dotnet restore"
            sh "dotnet build -c ${env.CONFIGURATION} --no-restore"
          }
        }
        stage("Make CLI packages"){
          steps {
            script {
              def sdf = sh(returnStdout: true, script: 'date -u +%Y%m%dT%H%M%S').trim()
              if (env.BRANCH_NAME == 'master') 
                env.DOTNET_ARGS = ""
              else
                env.DOTNET_ARGS = "--version-suffix SNAPSHOT" + sdf
            }
            sh "dotnet tool restore"
            sh "dotnet rpm -c ${env.CONFIGURATION} -r linux-x64 -f net5.0 ${env.DOTNET_ARGS} Terradue.OpenSearch.Client/Terradue.OpenSearch.Client.csproj"
            sh "dotnet deb -c ${env.CONFIGURATION} -r linux-x64 -f net5.0 ${env.DOTNET_ARGS} Terradue.OpenSearch.Client/Terradue.OpenSearch.Client.csproj"
            sh "dotnet zip -c ${env.CONFIGURATION} -r linux-x64 -f net5.0 ${env.DOTNET_ARGS} Terradue.OpenSearch.Client/Terradue.OpenSearch.Client.csproj"
            sh "dotnet publish -f net5.0 -r linux-x64 -p:PublishSingleFile=true ${env.DOTNET_ARGS} --self-contained true Terradue.OpenSearch.Client/Terradue.OpenSearch.Client.csproj"
            stash name: 'oscli-packages', includes: 'Terradue.OpenSearch.Client/bin/**/*.rpm'
            stash name: 'oscli-debs', includes: 'Terradue.OpenSearch.Client/bin/**/*.deb'
            stash name: 'oscli-exe', includes: 'Terradue.OpenSearch.Client/bin/**/linux*/publish/OpenSearchClient'
            stash name: 'oscli-zips', includes: 'Terradue.OpenSearch.Client/bin/**/linux*/*.zip'
            archiveArtifacts artifacts: 'Terradue.OpenSearch.Client/bin/linux**/publish/OpenSearchClient,Terradue.OpenSearch.Client/bin/linux**/publish/*.json,Terradue.OpenSearch.Client/bin/**/*.rpm,Terradue.OpenSearch.Client/bin/**/*.deb, Terradue.OpenSearch.Client/bin/**/*.zip', fingerprint: true
          }
        }
      }
    }
    stage('Publish Artifacts') {
      agent { node { label 'artifactory' } }
      steps {
        echo 'Deploying'
        unstash name: 'oscli-packages'
        script {
            // Obtain an Artifactory server instance, defined in Jenkins --> Manage:
            def server = Artifactory.server "repository.terradue.com"

            // Read the upload specs:
            def uploadSpec = readFile 'artifactdeploy.json'

            // Upload files to Artifactory:
            def buildInfo = server.upload spec: uploadSpec

            // Publish the merged build-info to Artifactory
            server.publishBuildInfo buildInfo
        }
      }       
    }
    stage('Build & Publish Docker') {
      steps {
        script {
          unstash name: 'oscli-debs'
          def starsdeb = findFiles(glob: "Terradue.OpenSearch.Client/bin/**/opensearch-client.*.linux-x64.deb")
          def descriptor = readDescriptor()
          sh "mv ${starsdeb[0].path} ."
          def mType=getTypeOfVersion(env.BRANCH_NAME)
          def testsuite = docker.build(descriptor.docker_image_name + ":${mType}${env.VERSION_TOOL}", "--no-cache --build-arg STARS_DEB=${starsdeb[0].name} .")
          testsuite.tag("${mType}latest")
          docker.withRegistry('https://registry.hub.docker.com', 'dockerhub') {
            testsuite.push("${mType}${env.VERSION_TOOL}")
            testsuite.push("${mType}latest")
          }
        }
      }
    }  
    stage('Create Release') {
      agent { 
          docker { 
              image 'golang:1.12'
              args '-u root'
          } 
      }
      when {
        branch 'master'
      }
      steps {
        withCredentials([string(credentialsId: '11f06c51-2f47-43be-aef4-3e4449be5cf0', variable: 'GITHUB_TOKEN')]) {
          unstash name: 'oscli-exe'
          unstash name: 'oscli-zips'
          sh "go get github.com/github-release/github-release"
          // echo "Deleting release from github before creating new one"
          // sh "github-release delete --user ${env.GITHUB_ORGANIZATION} --repo ${env.GITHUB_REPO} --tag ${env.VERSION_TOOL}"

          echo "Creating a new release in github"
          sh "github-release release --user ${env.GITHUB_ORGANIZATION} --repo ${env.GITHUB_REPO} --tag ${env.VERSION_TOOL} --name 'OpenSearch Client v${env.VERSION_TOOL}'"

          echo "Uploading the artifacts into github"
          sh "github-release upload --user ${env.GITHUB_ORGANIZATION} --repo ${env.GITHUB_REPO} --tag ${env.VERSION_TOOL} --name oscli-${env.VERSION_TOOL}-linux-x64 --file Terradue.OpenSearch.Client/bin/Release/net5.0/linux-x64/publish/OpenSearchClient"
          sh "github-release upload --user ${env.GITHUB_ORGANIZATION} --repo ${env.GITHUB_REPO} --tag ${env.VERSION_TOOL} --name oscli-${env.VERSION_TOOL}-linux-x64.zip --file Terradue.OpenSearch.Client/bin/Release/net5.0/linux-x64/opensearch-client.*.linux-x64.zip"
        }
      }
    }  
  }
}

def getTypeOfVersion(branchName) {
  def matcher = (branchName =~ /master/)
  if (matcher.matches())
    return ""
  
  return "dev"
}

def getConfiguration(branchName) {
  def matcher = (branchName =~ /master/)
  if (matcher.matches())
    return "Release"
  
  return "Debug"
}

def readDescriptor (){
  return readYaml(file: 'build.yml')
}

def getVersionFromCsProj (csProjFilePath){
  def file = readFile(csProjFilePath) 
  def xml = new XmlSlurper().parseText(file)
  def suffix = ""
  if ( xml.PropertyGroup.VersionSuffix[0].text() != "" )
    suffix = "-" + xml.PropertyGroup.VersionSuffix[0].text()
  return xml.PropertyGroup.Version[0].text() + suffix
}



