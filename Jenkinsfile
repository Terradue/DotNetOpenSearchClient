VERSION_TOOL=""

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
        DOTNET_CLI_HOME = '/tmp/DOTNET_CLI_HOME'
      }
      stages {
        stage('Build & Test') {
          steps {
            script {
              VERSION_TOOL = getVersionFromCsProj('Terradue.OpenSearch.Client/Terradue.OpenSearch.Client.csproj')
              echo 'Build .NET application'
              sh 'dotnet restore'
              sh "dotnet build -c ${env.CONFIGURATION} --no-restore"
              try {
                sh """dotnet test -c ${env.CONFIGURATION} --no-build --no-restore ./ --logger 'trx;LogFileName=testresults.trx' --filter 'Category!=NotWorking'
            """
            } catch (Exception e)
            {
                sh 'cat Terradue.OpenSearch.Client.Test/TestResults/testresults.trx'
                currentBuild.result = 'FAILURE'
                throw e
            }
            }
          }
        }
        stage('Make CLI packages') {
          steps {
            script {
              def sdf = sh(returnStdout: true, script: 'date -u +%Y%m%dT%H%M%S').trim()
              if (getConfiguration(env.BRANCH_NAME) == 'Release') {
                env.DOTNET_ARGS = ''
              } 
              else {
                env.DOTNET_ARGS = '--version-suffix SNAPSHOT' + sdf
              }
            }
            sh 'dotnet tool restore'
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
   stage('Create Release') { 
      when{
          branch pattern: "(release\\/[\\d.]+|master)", comparator: "REGEXP"
        }
      steps {
        script{
            withCredentials([string(credentialsId: '11f06c51-2f47-43be-aef4-3e4449be5cf0', variable: 'GITHUB_TOKEN')]) {
                unstash name: 'oscli-exe'
                unstash name: 'oscli-zips'
                //def releaseNotes = readFile(RELEASE_NOTES_FILE).trim()
                def apiUrl = "https://api.github.com/repos/Terradue/DotnetOpenSearchClient/releases"
                //https://api.github.com/repos/Terradue/DotnetOpenSearchClient/releases
                echo "${VERSION_TOOL}"
                echo '$VERSION_TOOL'
                def releaseBody = '''
                {
                    "tag_name": "${VERSION_TOOL}",
                    "target_commitish": "master",
                    "name": "${VERSION_TOOL}",
                    "body": "Release Notes",
                    "draft": false,
                    "prerelease": false
                }
                '''
                
                def curlCommand = "curl -X POST -H 'Authorization: token ${env.GITHUB_TOKEN}' -d '${releaseBody}' ${apiUrl}"
                sh curlCommand
                def ARTIFACT_PATH="Terradue.OpenSearch.Client/bin/Release/net5.0/linux-x64/opensearch-client.*.linux-x64.zip"
                // Upload artifact to release
                def uploadUrl = sh(script: "curl -s -H 'Authorization: token ${env.GITHUB_TOKEN}' ${apiUrl}/latest | grep upload_url | cut -d '\"' -f 4", returnStdout: true).trim()
                def ARTIFACT_NAME = sh(script: "ls ${ARTIFACT_PATH}", returnStdout: true).trim().split("/")
                ARTIFACT_NAME=ARTIFACT_NAME[-1]
                sh "curl -s -X POST -H 'Authorization: token ${env.GITHUB_TOKEN}' -H 'Content-Type: application/zip' --data-binary @\$(ls ${ARTIFACT_PATH}) '${uploadUrl}?name=${ARTIFACT_NAME}'"
                  //github-release release --user ${env.GITHUB_ORGANIZATION} --repo ${env.GITHUB_REPO} --tag ${VERSION_TOOL} --name 'OpenSearch Client v${VERSION_TOOL}'
                  //github-release upload --user ${env.GITHUB_ORGANIZATION} --repo ${env.GITHUB_REPO} --tag ${VERSION_TOOL} --name oscli-${VERSION_TOOL}-linux-x64 --file Terradue.OpenSearch.Client/bin/Release/net5.0/linux-x64/publish/OpenSearchClient
                  //github-release upload --user ${env.GITHUB_ORGANIZATION} --repo ${env.GITHUB_REPO} --tag ${VERSION_TOOL} --name oscli-${VERSION_TOOL}-linux-x64.zip --file Terradue.OpenSearch.Client/bin/Release/net5.0/linux-x64/opensearch-client.*.linux-x64.zip
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
            def server = Artifactory.server 'repository.terradue.com'

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
          def starsdeb = findFiles(glob: 'Terradue.OpenSearch.Client/bin/**/opensearch-client.*.linux-x64.deb')
          def descriptor = readDescriptor()
          sh "mv ${starsdeb[0].path} ."
          def mType = getTypeOfVersion(env.BRANCH_NAME)
          def testsuite = docker.build(descriptor.docker_image_name + ":${mType}${VERSION_TOOL}", "--no-cache --build-arg STARS_DEB=${starsdeb[0].name} .")
          testsuite.tag("${mType}latest")
          docker.withRegistry('https://registry.hub.docker.com', 'dockerhub') {
            testsuite.push("${mType}${VERSION_TOOL}")
            testsuite.push("${mType}latest")
          }
        }
      }
    }
 
  }
}

def getTypeOfVersion(branchName) {
  def matcher = (branchName =~ /(release\/[\d.]+|master)/)
  if (matcher.matches()) {
    return ''
  }

  return 'dev'
}

def getConfiguration(branchName) {
  def matcher = (branchName =~ /(release\/[\d.]+|master)/)
  if (matcher.matches()) {
    return 'Release'
  }

  return 'Debug'
}

def readDescriptor () {
  return readYaml(file: 'build.yml')
}

def getVersionFromCsProj (csProjFilePath) {
  def file = readFile(csProjFilePath)
  def xml = new XmlSlurper().parseText(file)
  def suffix = ''
  if ( xml.PropertyGroup.VersionSuffix[0].text() != '' ) {
    suffix = '-' + xml.PropertyGroup.VersionSuffix[0].text()
  }
  return xml.PropertyGroup.Version[0].text() + suffix
}
