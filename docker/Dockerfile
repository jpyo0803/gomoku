FROM ubuntu:24.04

ENV DEBIAN_FRONTEND=noninteractive

# 기본 패키지 설치
RUN apt-get update && apt-get install -y \
    zsh \
    curl \
    git \
    vim \
    locales \
    sudo \
    python3 \
    python3-pip \
    build-essential \
    ca-certificates \
    tree \
    cmake \
    clang-format \
    tmux \
    redis \
    postgresql \
    postgresql-contrib \
 && apt-get clean

# locale 설정
RUN locale-gen en_US.UTF-8
ENV LANG=en_US.UTF-8
ENV LANGUAGE=en_US:en
ENV LC_ALL=en_US.UTF-8

# Node.js LTS 설치 (20.x 기준)
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    npm install -g typescript

# 비root 사용자 생성
RUN useradd -ms /bin/zsh user && echo "user ALL=(ALL) NOPASSWD:ALL" >> /etc/sudoers

# 사용자 전환
USER user
WORKDIR /home/user

# oh-my-zsh 설치
RUN sh -c "$(curl -fsSL https://gitee.com/mcornella/ohmyzsh/raw/master/tools/install.sh)" "" --unattended

# zsh 플러그인 설치
RUN git clone https://github.com/zsh-users/zsh-autosuggestions.git ~/.oh-my-zsh/custom/plugins/zsh-autosuggestions && \
    git clone https://github.com/zsh-users/zsh-syntax-highlighting.git ~/.oh-my-zsh/custom/plugins/zsh-syntax-highlighting

# .zshrc 수정
RUN sed -i 's/ZSH_THEME=".*"/ZSH_THEME="robbyrussell"/' ~/.zshrc && \
    sed -i 's/plugins=(git)/plugins=(git zsh-autosuggestions zsh-syntax-highlighting)/' ~/.zshrc

# 기본 셸을 zsh로 설정
RUN sudo chsh -s $(which zsh) user

# 시작 시 zsh 실행
CMD ["zsh"]
