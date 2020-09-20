pipeline {
  parameters{
    booleanParam(name: 'RUN_TESTS', defaultValue: false, description: 'If this parameter is set, the tests will run during the build.', )
    string(name: 'TEST_AUTH', defaultValue: '', description: 'Add &lt;name&gt;=&lt;username&gt;:&lt;password&gt; triples separated by spaces. If not specified, tests requring authentication are skipped.', )
    choice(name: 'DOTNET_CONFIG', choices: "Debug\nRelease", description: 'Debug will produce symbols in the assmbly to be able to debug it at runtime. This is the recommended option for feature, hotfix testing or release candidate.<br/><strong>For publishing a release from master branch, please choose Release.</strong>', )
   }
  agent any
  stages {
    stage('Build') {
      agent { 
          docker { 
              image 'mono:6.8' 
          } 
      }
      environment {
          HOME = '$WORKSPACE'
      }
      steps {
        echo "Build .NET application"
        sh "msbuild /t:build /p:Configuration=DEBUG /Restore:true"
        stash includes: 'Terradue.OpenSearch.Client/bin/**', name: 'opensearch-client-build'
      }
    }
    stage('Package') {
      agent { 
            docker { 
                image 'alectolytic/rpmbuilder:centos-7' 
            } 
        }
      steps {
        unstash name: 'opensearch-client-build'
        script {
          def sdf = sh(returnStdout: true, script: 'date -u +%Y%m%dT%H%M%S').trim()
          if (env.BRANCH_NAME == 'master') 
            env.release = env.BUILD_NUMBER
          else
            env.release = "SNAPSHOT" + sdf
        }
        sh 'mkdir -p $WORKSPACE/build/{BUILD,RPMS,SOURCES,SPECS,SRPMS}'
        sh 'mkdir -p $WORKSPACE/build/SOURCES/usr/lib/opensearch-client'
        sh 'cp -r Terradue.OpenSearch.Client/bin/Debug/net4.5/* $WORKSPACE/build/SOURCES/usr/lib/opensearch-client/'
        sh 'mkdir -p $WORKSPACE/build/SOURCES/usr/bin'
        sh 'cp src/main/scripts/opensearch-client $WORKSPACE/build/SOURCES/usr/bin'
        sh 'cp src/main/scripts/opensearch-client $WORKSPACE/build/SOURCES/'
        sh 'cp opensearch-client.spec $WORKSPACE/build/SPECS/opensearch-client.spec'
        sh 'spectool -g -R --directory $WORKSPACE/build/SOURCES $WORKSPACE/build/SPECS/opensearch-client.spec'
        echo "Build package"
        sh "rpmbuild --define \"_topdir $WORKSPACE/build\" -ba --define '_branch ${env.BRANCH_NAME}' --define '_release ${env.release}' $WORKSPACE/build/SPECS/opensearch-client.spec"
        sh "rpm -qpl $WORKSPACE/build/RPMS/*/*.rpm"
        sh 'rm -f $WORKSPACE/build/SOURCES/opensearch-client'
        sh "tar -cvzf opensearch-client-1.9.7-${env.release}.tar.gz -C $WORKSPACE/build/SOURCES/ ."
        archiveArtifacts artifacts: 'build/RPMS/**/*.rpm,opensearch-client-*.tar.gz', fingerprint: true
        stash includes: 'opensearch-client-*.tar.gz', name: 'opensearch-client-tgz'
        stash includes: 'build/RPMS/**/*.rpm', name: 'opensearch-client-rpm'
      }
    }
    stage('Publish') {
      agent { label 'artifactory' }
      steps {
        echo 'Deploying'
        unstash name: 'opensearch-client-rpm'
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
            unstash name: 'opensearch-client-tgz'
            script {
              def opensearchclienttgz = findFiles(glob: "opensearch-client-*.tar.gz")
              def testsuite = docker.build("terradue/opensearch-client", "--build-arg OPENSEARCH_CLIENT_TGZ=${opensearchclienttgz[0].name} .")
              def mType=getTypeOfVersion(env.BRANCH_NAME)
              docker.withRegistry('https://registry.hub.docker.com', 'dockerhub-emmanuelmathot') {
                testsuite.push("${mType}${descriptor.version}")
                testsuite.push("${mType}latest")
              }
            }
        }
    }
  }
}

def getTypeOfVersion(branchName) {
  
  def matcher = (env.BRANCH_NAME =~ /master/)
  if (matcher.matches())
    return ""
  
  return "dev"
}
